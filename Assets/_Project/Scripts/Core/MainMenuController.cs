using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceMining
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private LevelCatalog catalog;

        public void OnPlayPressed()
        {
            LevelSelection.CurrentIndex = 0;
            LevelSelection.Current = catalog != null ? catalog.GetAt(0) : null;
            LivesManager.ResetLives();
            SceneManager.LoadScene(SceneNames.GameLevel);
        }

        public void OnSettingsPressed()
        {
            settingsPanel.SetActive(true);
        }

        public void OnSettingsClose()
        {
            settingsPanel.SetActive(false);
        }
    }
}
