// Tasks 3.2 + 3.3 + 3.4 — Game State
// Space Mining Logistics

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace SpaceMining
{
    public class GameManager : MonoBehaviour
    {
        public SlotManager slotManager;
        public GridManager gridManager;
        public GameObject levelClearedPanel;
        public GameObject levelFailedPanel;
        public GameObject levelClearEffectPrefab;
        public LevelCatalog catalog;

        private int _pauseStack = 0;

        public void RequestPause()
        {
            _pauseStack++;
            Time.timeScale = 0f;
        }

        public void ReleasePause()
        {
            _pauseStack = Mathf.Max(0, _pauseStack - 1);
            if (_pauseStack == 0) Time.timeScale = 1f;
        }

        public bool CanAnyShipMine()
        {
            foreach (var slot in slotManager.Slots)
            {
                if (slot.IsEmpty) continue;
                if (gridManager.GetTargetableBlocksOfType(slot.Ship.color).Count > 0)
                    return true;
            }
            return false;
        }

        public bool IsQueueEmpty()
        {
            foreach (var col in gridManager.Columns)
                if (!col.IsEmpty) return false;
            return true;
        }

        public bool AreAllSlotsEmpty()
        {
            foreach (var slot in slotManager.Slots)
                if (!slot.IsEmpty) return false;
            return true;
        }

        public bool AreAllOreMined()
        {
            for (int x = 0; x < gridManager.Width; x++)
                for (int y = 0; y < gridManager.Height; y++)
                {
                    var block = gridManager.GetBlock(x, y);
                    if (block != null && !block.isMined) return false;
                }
            return true;
        }

        public bool IsOverlayActive =>
            (levelFailedPanel  != null && levelFailedPanel.activeSelf) ||
            (levelClearedPanel != null && levelClearedPanel.activeSelf);

        public void CheckOreMined()
        {
            if (!AreAllOreMined()) return;
            StartCoroutine(ShowLevelClearedAfterDelay());
        }

        private IEnumerator ShowLevelClearedAfterDelay()
        {
            yield return new WaitForSecondsRealtime(0.35f);
            Debug.Log("[GameManager] LEVEL CLEARED");
            if (levelClearEffectPrefab != null)
                Instantiate(levelClearEffectPrefab, Vector3.zero, Quaternion.identity);
            RequestPause();
            AudioManager.PlayLevelClear();
            if (levelClearedPanel != null) levelClearedPanel.SetActive(true);
        }

        public void EvaluateState()
        {
            if (slotManager.AreAllSlotsFull() && !CanAnyShipMine() && !IsQueueEmpty())
            {
                Debug.Log("[GameManager] GAME OVER");
                RequestPause();
                AudioManager.PlayGameOver();
                if (levelFailedPanel != null) levelFailedPanel.SetActive(true);
            }
        }

        public void LoadNextLevel()
        {
            _pauseStack = 0;
            Time.timeScale = 1f;

            LevelData next = catalog != null ? catalog.GetNext(LevelSelection.CurrentIndex) : null;
            if (next != null)
            {
                LevelSelection.CurrentIndex++;
                LevelSelection.Current = next;
                SceneManager.LoadScene(SceneNames.GameLevel);
            }
            else
            {
                SceneManager.LoadScene(SceneNames.MainMenu);
            }
        }

        public void RestartLevel()
        {
            _pauseStack = 0;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void GoToMainMenu()
        {
            _pauseStack = 0;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
