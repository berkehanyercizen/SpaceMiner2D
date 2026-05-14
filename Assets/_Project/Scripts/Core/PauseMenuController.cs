using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceMining
{
    public class PauseMenuController : MonoBehaviour
    {
        public GameObject pausePanel;
        public GameManager gameManager;

        private bool _paused;

        public void OnPausePressed()
        {
            if (gameManager != null && gameManager.IsOverlayActive) return;
            _paused = true;
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
        }

        public void OnResumePressed()
        {
            _paused = false;
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }

        public void OnHomePressed()
        {
            gameManager?.GoToMainMenu();
        }

        public void OnRestartPressed()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
