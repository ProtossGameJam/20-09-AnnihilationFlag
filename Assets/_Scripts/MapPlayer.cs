using ProjectAF.Crowd;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapData = System.Collections.Generic.Dictionary<ProjectAF.TilePosition, ProjectAF.TileType>;

namespace ProjectAF
{
    public class MapPlayer : MonoBehaviour
    {
        [SerializeField]
        protected Tilemap tilemap = null;

        [SerializeField]
        protected CrowdController crowdController = null;

        [SerializeField]
        protected List<TileData> tiles = null;

        [SerializeField]
        protected GameObject destinationPrefab = null;

        [SerializeField]
        protected Transform destinationParent = null;

        [SerializeField]
        protected GameObject trapPrefab = null;

        [SerializeField]
        protected Transform trapParent = null;

        [SerializeField]
        protected new Camera camera = null;

        [SerializeField]
        private RectTransform clickableArea = null;

        [SerializeField]
        protected GameObject rangeCircle = null;

        [SerializeField]
        protected float cameraMoveSpeed = 1;

        [SerializeField]
        protected float cameraWheelSpeed = 10;

        protected MapData _data;

        protected readonly List<Vector3> destinations = new List<Vector3>();

        private readonly List<GameObject> _destinationColliders = new List<GameObject>();

        protected readonly List<Vector3> traps = new List<Vector3>();

        private readonly List<GameObject> _trapColliders = new List<GameObject>();

        private readonly Queue<Vector3Int> _blueFlagCooldownQueue = new Queue<Vector3Int>();

        public bool interactable = true;

        protected bool isStartingPointExists = false;

        protected Vector3 startingPoint;

        private readonly WaitForSeconds _blueFlagCooldown = new WaitForSeconds(3f);

        public bool playMode = false;

        private bool _isTutorial = true;

        protected virtual void Update()
        {
            if (!interactable)
            {
                return;
            }

            var wheel = Input.GetAxis("Mouse ScrollWheel") * cameraWheelSpeed;

            if (camera.orthographicSize + wheel >= 15f)
            {
                camera.orthographicSize = 15f;
            }
            else if (camera.orthographicSize + wheel < 4f)
            {
                camera.orthographicSize = 4f;
            }
            else if (!Mathf.Approximately(wheel, 0.0f))
            {
                camera.orthographicSize += wheel;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                camera.transform.position = new Vector3(0f, 0f, -10f);
            }

            var xAxis = Input.GetAxis("Horizontal");
            var yAxis = Input.GetAxis("Vertical");
            var delta = new Vector2(xAxis, yAxis) * Time.deltaTime * cameraMoveSpeed;
            camera.transform.Translate(delta);

            var mousePos = Input.mousePosition;

            if (CheckClickableArea(mousePos))
            {
                var (position, type) = GetMouseOveredTile(mousePos);

                var isFlag = type == TileType.RedFlag || type == TileType.BlueFlag;
                if (isFlag)
                {
                    rangeCircle.transform.position = position;
                }
                rangeCircle.gameObject.SetActive(isFlag);

                if (playMode)
                {
                    var isMouseDown = Input.GetMouseButtonDown(0);
                    var isMouseUp = Input.GetMouseButtonUp(0);

                }
            }
        }

        public virtual void LoadMapData(FileInfo fileInfo)
        {
            var mapData = MapDataManager.Load(fileInfo);
            if (mapData is null)
            {
                Debug.LogError("Error loading map!");
                return;
            }

            _data = mapData;
            tilemap.ClearAllTiles();
            foreach (var collider in _destinationColliders)
            {
                collider.SetActive(false);
            }
            destinations.Clear();
            foreach (var collider in _trapColliders)
            {
                collider.SetActive(false);
            }
            traps.Clear();
            startingPoint = Vector3Int.zero;
            isStartingPointExists = false;

            foreach (var key in _data.Keys)
            {
                var position = new Vector3Int(key.x, key.y, 0);
                var tile = tiles.FirstOrDefault(t => t.tileType == _data[key]);
                tilemap.SetTile(position, tile.tile);

                if (tile.tileType == TileType.StartingPoint)
                {
                    OnStartingPointTileAdded(position);
                }
                else if (tile.tileType == TileType.Destination)
                {
                    OnDestinationTileAdded(position);
                }
                else if (tile.tileType == TileType.Trap)
                {
                    OnTrapAdded(position);
                }
            }

            interactable = true;
        }

        public bool CheckCoolDown(Vector3Int position)
        {
            return _blueFlagCooldownQueue.Contains(position);
        }

        public void EnqueueCooldown(Vector3Int position)
        {
            _blueFlagCooldownQueue.Enqueue(position);
            StartCoroutine(CoHandleCooldown());
        }

        public void EndTutorial()
        {
            _isTutorial = false;
        }

        public void PlayStart()
        {
            crowdController.SetTileData(tilemap, startingPoint);
            var startingCameraPos = startingPoint;
            startingCameraPos.z = -10f;
            camera.transform.position = startingCameraPos;
            crowdController.MixCrowds();
        }

        private IEnumerator CoHandleCooldown()
        {
            yield return _blueFlagCooldown;
            _blueFlagCooldownQueue.Dequeue();
        }

        protected (Vector3 position, TileType tileType) GetMouseOveredTile(Vector2 mousePosition)
        {
            var grid = tilemap.layoutGrid;
            var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            var gridPosition = grid.WorldToCell(worldPosition);
            var tile = tilemap.GetTile(gridPosition);

            var tileData = tiles.FirstOrDefault(t => t.tile.Equals(tile));
            return (gridPosition + tilemap.tileAnchor,
                tileData.tile is null ? TileType.None : tileData.tileType);
        }

        protected virtual void OnStartingPointTileAdded(Vector3Int position)
        {
            startingPoint = position;
            isStartingPointExists = true;
            
        }

        protected virtual void OnDestinationTileAdded(Vector3Int position)
        {
            destinations.Add(position);

            var colliderObject = _destinationColliders.FirstOrDefault(x => !x.activeSelf);

            if (colliderObject is null)
            {
                var go = Instantiate(destinationPrefab,
                    position + tilemap.tileAnchor,
                    Quaternion.identity,
                    destinationParent);
                _destinationColliders.Add(go);
            }
            else
            {
                colliderObject.SetActive(true);
                colliderObject.transform.position = position + tilemap.tileAnchor;
            }
        }

        protected virtual void OnDestinationTileRemoved(Vector3Int position)
        {
            destinations.Remove(position);

            var colliderObject = _destinationColliders
                .FirstOrDefault(x => x.transform.position == position + tilemap.tileAnchor);
            if (colliderObject is null)
            {
                Debug.LogError("Failed to erase destination collider!");
                return;
            }
            colliderObject.SetActive(false);
        }

        protected virtual void OnTrapAdded(Vector3Int position)
        {
            traps.Add(position);

            var colliderObject = _trapColliders.FirstOrDefault(x => !x.activeSelf);

            if (colliderObject is null)
            {
                var go = Instantiate(trapPrefab,
                    position + tilemap.tileAnchor,
                    Quaternion.identity,
                    trapParent);
                _trapColliders.Add(go);
            }
            else
            {
                colliderObject.SetActive(true);
                colliderObject.transform.position = position + tilemap.tileAnchor;
            }
        }

        protected virtual void OnTrapRemoved(Vector3Int position)
        {
            traps.Remove(position);

            var colliderObject = _trapColliders
                .FirstOrDefault(x => x.transform.position == position + tilemap.tileAnchor);
            if (colliderObject is null)
            {
                Debug.LogError("Failed to erase trap collider!");
                return;
            }
            colliderObject.SetActive(false);
        }

        protected bool CheckClickableArea(Vector2 mousePosition)
        {
            return RectTransformUtility
                .RectangleContainsScreenPoint(clickableArea, mousePosition);
        }
    }
}
