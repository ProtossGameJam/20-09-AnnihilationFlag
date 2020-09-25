using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProjectAF.Crowd
{
    public class CrowdController : MonoBehaviour
    {
        public static CrowdController Instance = null;

        [SerializeField]
        private int GoalCount = 5;

        [SerializeField]
        private int goal;

        [SerializeField]
        private List<TileData> tiles = null;

        [SerializeField]
        private Vector3 startingPoint;

        [SerializeField]
        private MapPlayer mapPlayer = null;

        [SerializeField]
        private Popup clearPopup = null;

        [SerializeField]
        private AudioSource audioSource = null;

        private float _mixRange = 0.5f;

        private Sheep[] _crowd;

        private Tilemap _tilemap;

        private bool _interactable = true;

        protected virtual void Awake()
        {
            Instance = this;
        }

        public void SetTileData(Tilemap tilemap, Vector3 startingPoint)
        {
            if (tilemap is null)
            {
                Debug.LogError("Tilemap is null!");
                return;
            }
            _tilemap = tilemap;
            this.startingPoint = startingPoint;
        }

        private void Update()
        {
            if (!_interactable)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                MixCrowds();
            }

            var mousePos = Input.mousePosition;

            var isMouseDown = Input.GetMouseButtonDown(0);
            var isMouseUp = Input.GetMouseButtonUp(0);

            if (isMouseDown || isMouseUp)
            {
                var tileInfo = GetMouseOveredTile(mousePos);

                switch (tileInfo.tileType)
                {
                    case TileType.Ground:
                        break;
                    case TileType.RedFlag:
                        if (isMouseDown)
                        {
                            RequestStartMoving(tileInfo.position);
                        }
                        else
                        {
                            RequestStopMoving();
                        }
                        break;
                    case TileType.BlueFlag:
                        if (isMouseDown)
                        {
                            RequestJump(tileInfo.position);
                        }
                        break;
                    case TileType.Destination:
                        break;
                    case TileType.StartingPoint:
                        break;
                }
            }
        }

        private (Vector3Int position, TileType tileType) GetMouseOveredTile(Vector2 mousePosition)
        {
            var grid = _tilemap.layoutGrid;
            var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            var gridPosition = grid.WorldToCell(worldPosition);
            var tile = _tilemap.GetTile(gridPosition);

            var tileData = tiles.FirstOrDefault(t => t.tile.Equals(tile));
            return (gridPosition, tileData.tileType);
        }

        public void RequestStartMoving(Vector3 destination)
        {
            foreach (var sheep in _crowd)
            {
                if (!sheep.gameObject.activeSelf)
                    continue;

                sheep.StartMoving(destination);
            }
        }

        public void RequestStopMoving()
        {
            foreach (var sheep in _crowd)
            {
                if (!sheep.gameObject.activeSelf)
                    continue;

                sheep.StopMoving();
            }
        }

        public void RequestJump(Vector3Int destination)
        {
            if (mapPlayer.CheckCoolDown(destination))
            {
                return;
            }

            mapPlayer.EnqueueCooldown(destination);
            foreach (var sheep in _crowd)
            {
                sheep.Jump(destination);
            }

            var activeCount = _crowd.Count(x => x.gameObject.activeSelf);
            for (int i = 0; i < activeCount; ++i)
            {
                var delay = Random.Range(0f, 0.3f);
                StartCoroutine(CoPlaySFXDelayed(delay));
            }
        }

        private IEnumerator CoPlaySFXDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            audioSource.PlayOneShot(audioSource.clip);
        }

        public Vector3 GetAveragePosition()
        {
            var vec = Vector3.zero;

            foreach (var sheep in _crowd)
            {
                if (!sheep.gameObject.activeSelf)
                    continue;
                vec += sheep.transform.position;
            }

            return vec / _crowd.Count(x => x.gameObject.activeSelf);
        }

        public void MixCrowds()
        {
            if (_crowd is null)
            {
                _crowd = GetComponentsInChildren<Sheep>();
            }

            foreach (var sheep in _crowd)
            {
                var x = Random.Range(-_mixRange, _mixRange) * 2f;
                var y = Random.Range(-_mixRange, _mixRange);
                var z = sheep.transform.position.z;

                var pos = startingPoint + new Vector3(x, y);
                pos.z = z;

                var r2d = sheep.GetComponent<Rigidbody2D>();
                r2d.velocity = Vector2.zero;

                sheep.transform.position = pos;
                sheep.gameObject.SetActive(true);
            }

            goal = GoalCount;
        }

        public void OnEnterGoal(GameObject sheep)
        {
            sheep.SetActive(false);
            --goal;
            if (goal <= 0)
            {
                mapPlayer.EndTutorial();
                mapPlayer.interactable = false;
                _interactable = false;
                clearPopup.OnClose = () =>
                {
                    mapPlayer.interactable = true;
                    _interactable = true;
                };
                clearPopup.Show("클리어!");
            }
        }
    }
}
