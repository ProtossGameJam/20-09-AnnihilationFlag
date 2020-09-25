using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ProjectAF.UI
{
    public class FileExplorer : MonoBehaviour
    {
        [SerializeField]
        private GameObject cellViewPrefab = null;

        [SerializeField]
        private Transform content = null;

        private FileInfo selectedFile = null;

        private readonly Dictionary<string, MapInfoCellView> cellViewMap
            = new Dictionary<string, MapInfoCellView>();

        public Action<FileInfo> OnMapLoadRequested;

        public Action OnHide = null;

        public void Show()
        {
            gameObject.SetActive(true);

            var filePathes = Directory.GetFiles(GameConfig.MapDataPath + "/", "*.afm");

            foreach (var filePath in filePathes)
            {
                var fileInfo = new FileInfo(filePath);
                var contains = cellViewMap.ContainsKey(fileInfo.Name);

                var cellView = contains ?
                    cellViewMap[fileInfo.Name] :
                    Instantiate(cellViewPrefab, content).GetComponent<MapInfoCellView>();
                if (!contains)
                {
                    cellViewMap[fileInfo.Name] = cellView;
                }

                cellView.SetData(
                    fileInfo.Name,
                    fileInfo.LastWriteTime,
                    fileInfo.Length,
                    () => selectedFile = fileInfo);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            OnHide?.Invoke();
        }

        public void LoadMap()
        {
            OnMapLoadRequested?.Invoke(selectedFile);
        }
    }
}
