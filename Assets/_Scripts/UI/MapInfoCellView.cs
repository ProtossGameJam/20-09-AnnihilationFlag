using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectAF.UI
{
    public class MapInfoCellView : MonoBehaviour
    {
        [SerializeField]
        private Text mapNameText = null;

        [SerializeField]
        private Text editedTimeText = null;

        [SerializeField]
        private Text fileSizeText = null;

        [SerializeField]
        private Button button = null;

        private Action OnClick = null;

        public string MapName = null;

        private const string editTimeFormat = "마지막 수정 시간 : {0}";

        private const string fileSizeFormat = "파일 크기 : {0}B";

        private void Awake()
        {
            button.onClick.AddListener(() => OnClick?.Invoke());
        }

        public void SetData(string mapName, DateTime editedTime, long fileSize, Action onClick)
        {
            MapName = mapName;
            mapNameText.text = mapName;
            var editedTimeString = editedTime.ToString("yyyy-MM-dd HH:mm:ss");
            editedTimeText.text = string.Format(editTimeFormat, editedTimeString);
            fileSizeText.text = string.Format(fileSizeFormat, fileSize);
            OnClick = onClick;
        }
    }
}
