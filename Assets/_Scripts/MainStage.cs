using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ProjectAF
{
    public class MainStage : MonoBehaviour
    {
        [SerializeField]
        private MapPlayer mapPlayer = null;

        [SerializeField]
        private Popup clearPopup = null;

        private readonly List<FileInfo> _stageFileInfos = new List<FileInfo>();

        private int _currentStage = 0;

        private int _stageCount = 0;

        protected virtual void Awake()
        {
            _currentStage = 0;
            var filePathes = Directory.GetFiles(GameConfig.MainMapDataPath + "/", "*.afm");
            foreach (var filePath in filePathes)
            {
                var fileInfo = new FileInfo(filePath);
                _stageFileInfos.Add(fileInfo);
            }
            _stageCount = _stageFileInfos.Count;

            mapPlayer.LoadMapData(_stageFileInfos[_currentStage]);
            mapPlayer.PlayStart();
        }

        public void ProceedNextStage()
        {
            ++_currentStage;
            if (_currentStage >= _stageCount)
            {
                _currentStage = _stageCount - 1;
            }

            mapPlayer.LoadMapData(_stageFileInfos[_currentStage]);
            mapPlayer.PlayStart();
            clearPopup.Hide();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (clearPopup.gameObject.activeSelf)
                {
                    clearPopup.Hide();
                    return;
                }

                clearPopup.OnClose = () => Time.timeScale = 1f;
                clearPopup.Show("시간아 멈춰라!");
                Time.timeScale = 0f;
            }
        }

        public void ProceedPreviousStage()
        {
            --_currentStage;
            if (_currentStage <= 0)
            {
                _currentStage = 0;
            }

            mapPlayer.LoadMapData(_stageFileInfos[_currentStage]);
            mapPlayer.PlayStart();
            clearPopup.Hide();
        }
    }
}

