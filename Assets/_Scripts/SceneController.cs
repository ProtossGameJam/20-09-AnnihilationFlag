using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectAF
{
    public class SceneController : MonoBehaviour
    {
        public void LoadMainStage()
        {
            SceneManager.LoadScene("MainStage");
            Time.timeScale = 1f;
        }

        public void LoadMapEditor()
        {
            SceneManager.LoadScene("MapEditor");
            Time.timeScale = 1f;
        }

        public void LoadMainScene()
        {
            SceneManager.LoadScene("Lobby");
            Time.timeScale = 1f;
        }

        public void GoodBye()
        {
            Application.Quit();
        }
    }
}
