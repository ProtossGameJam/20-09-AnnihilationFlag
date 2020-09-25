using ProjectAF.UI;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapData = System.Collections.Generic.Dictionary<ProjectAF.TilePosition, ProjectAF.TileType>;

namespace ProjectAF
{
    [Serializable]
    public struct TileData
    {
        public TileBase tile;
        public TileType tileType;
    }

    public class MapEditor : MapPlayer
    {
        [SerializeField]
        private GameObject tilePalette = null;

        [SerializeField]
        private SpriteRenderer cursor = null;

        [SerializeField]
        private ConfirmPopup saveConfirmPopup = null;

        [SerializeField]
        private InputPopup savePopup = null;

        [SerializeField]
        private Popup pausePopup = null;

        [SerializeField]
        private FileExplorer fileExplorer = null;

        [SerializeField]
        private TileBase _startingPointTile = null;

        [SerializeField]
        private TileBase _destinationTile = null;

        [SerializeField]
        private TileBase _trapTile = null;

        private TileData _currentTileBrush;

        protected virtual void Awake()
        {
            _currentTileBrush = tiles[0];
            _data = new MapData();

            fileExplorer.OnMapLoadRequested = LoadMapData;
        }
            
        protected override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (pausePopup.gameObject.activeSelf)
                {
                    pausePopup.Hide();
                }
                pausePopup.OnClose = () => interactable = true;
                pausePopup.Show();
                interactable = false;
                return;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                TogglePlayMode();
            }

            if (!interactable)
            {
                cursor.gameObject.SetActive(false);
                return;
            }

            base.Update();

            if (playMode)
            {
                cursor.gameObject.SetActive(false);
                return;
            }

            var mousePos = Input.mousePosition;

            if (CheckClickableArea(mousePos))
            {
                cursor.gameObject.SetActive(true);
                UpdateCursor(mousePos);

                if (Input.GetMouseButton(0))
                {
                    if (CheckClickableArea(mousePos))
                        Paint(mousePos);
                }
                else if (Input.GetMouseButton(1))
                {
                    if (CheckClickableArea(mousePos))
                        Erase(mousePos);
                }
            }
            else
            {
                cursor.gameObject.SetActive(false);
            }
        }

        protected void UpdateCursor(Vector2 mousePosition)
        {
            var grid = tilemap.layoutGrid;
            var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            var gridPosition = grid.WorldToCell(worldPosition);

            cursor.transform.position = gridPosition + tilemap.tileAnchor;
        }

        protected void Paint(Vector2 mousePosition)
        {
            if (_currentTileBrush.tileType == TileType.StartingPoint && isStartingPointExists)
            {
                return;
            }

            var grid = tilemap.layoutGrid;
            var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            var gridPosition = grid.WorldToCell(worldPosition);
            var previousTile = tilemap.GetTile(gridPosition);

            if (previousTile is null || !previousTile.Equals(_currentTileBrush.tile))
            {
                if (_currentTileBrush.tileType == TileType.StartingPoint)
                {
                    OnStartingPointTileAdded(gridPosition);
                }
                else if (_currentTileBrush.tileType == TileType.Destination)
                {
                    OnDestinationTileAdded(gridPosition);
                }
                else if (_currentTileBrush.tileType == TileType.Trap)
                {
                    OnTrapAdded(gridPosition);
                }

                tilemap.SetTile(gridPosition, _currentTileBrush.tile);
                var tilePosition = new TilePosition(gridPosition.x, gridPosition.y);
                _data[tilePosition] = _currentTileBrush.tileType;
            }
        }

        protected void Erase(Vector2 mousePosition)
        {
            var grid = tilemap.layoutGrid;
            var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            var gridPosition = grid.WorldToCell(worldPosition);
            var previousTile = tilemap.GetTile(gridPosition);

            if (!(previousTile is null))
            {
                if (previousTile.Equals(_startingPointTile))
                {
                    isStartingPointExists = false;
                }
                else if (previousTile.Equals(_destinationTile))
                {
                    OnDestinationTileRemoved(gridPosition);
                }
                else if (previousTile.Equals(_trapTile))
                {
                    OnTrapRemoved(gridPosition);
                }

                tilemap.SetTile(gridPosition, null);
                var tilePosition = new TilePosition(gridPosition.x, gridPosition.y);
                _data.Remove(tilePosition);
            }
        }

        public override void LoadMapData(FileInfo fileInfo)
        {
            base.LoadMapData(fileInfo);
            fileExplorer.Hide();
        }

        public void SwapTileBrush(int index)
        {
            var selectedTile = tiles[index];
            _currentTileBrush = selectedTile;
            switch (selectedTile.tile)
            {
                case Tile tile:
                    cursor.sprite = tile.sprite;
                    break;
                case AnimatedTile animatedTile:
                    if (animatedTile.m_AnimatedSprites.Length > 0)
                    {
                        cursor.sprite = animatedTile.m_AnimatedSprites[0];
                    }
                    else
                    {
                        cursor.sprite = null;
                    }
                    break;
                case RuleTile ruleTile:
                    cursor.sprite = ruleTile.m_DefaultSprite;
                    break;
            }
        }

        public void SetInteractable(bool value)
        {
            interactable = value;
        }

        public void OpenFileExplorer()
        {
            fileExplorer.Show();
            interactable = false;
            fileExplorer.OnHide = () =>
            {
                interactable = true;
            };
        }

        public void CreateNewMapData()
        {
            interactable = false;

            saveConfirmPopup.Show(() =>
            {
                savePopup.Show(name =>
                {
                    SaveMapData(name);
                    _data = new MapData();
                    tilemap.ClearAllTiles();
                },
                () => interactable = true);
            },
            () => interactable = true);
        }

        public void ShowSavePopup()
        {
            interactable = false;
            savePopup.Show(SaveMapData,
            () =>
            {
                interactable = true;
            });
        }

        public void SaveMapData(string name)
        {
            if (!MapDataManager.Save(_data, name))
            {
                return;
            }

            savePopup.Hide();
            interactable = true;
        }

        private void TogglePlayMode()
        {
            playMode = isStartingPointExists ? !playMode : false;
            tilePalette.SetActive(!playMode);

            if (playMode)
            {
                crowdController.gameObject.SetActive(true);
                PlayStart();
            }
            else
            {
                crowdController.gameObject.SetActive(false);
            }
        }
    }
}
