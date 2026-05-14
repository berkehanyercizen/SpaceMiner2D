using System.Collections;
using UnityEngine;

namespace SpaceMining
{
    public class LevelFailedController : MonoBehaviour
    {
        [Header("References")]
        public GameManager gameManager;
        public SlotManager slotManager;

        [Header("Panels")]
        public GameObject adPlaceholderPanel;

        [Header("Ad Close Button")]
        public GameObject closeAdButton;

        private Coroutine _closeButtonCoroutine;

        // --- Main failed-screen buttons ---

        public void OnRestartPressed()
        {
            LivesManager.ConsumeLife();
            gameManager?.RestartLevel();
        }

        // Shows the ad popup and starts the 5-second close-button timer
        public void OnContinueAdPressed()
        {
            if (adPlaceholderPanel == null) return;

            adPlaceholderPanel.SetActive(true);

            if (closeAdButton != null)
                closeAdButton.SetActive(false);

            if (_closeButtonCoroutine != null)
                StopCoroutine(_closeButtonCoroutine);

            _closeButtonCoroutine = StartCoroutine(ShowCloseButtonAfterDelay(5f));
        }

        private IEnumerator ShowCloseButtonAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (closeAdButton != null)
                closeAdButton.SetActive(true);
        }

        // --- Ad placeholder popup buttons ---

        public void OnWatchAdConfirmed()
        {
            slotManager?.ChangeSlotCount(1);
            CloseAdPanel();
            gameObject.SetActive(false);
            gameManager?.ReleasePause();
        }

        public void OnWatchAdDeclined()
        {
            CloseAdPanel();
        }

        public void OnCloseAdPressed()
        {
            CloseAdPanel();
        }

        private void CloseAdPanel()
        {
            if (_closeButtonCoroutine != null)
            {
                StopCoroutine(_closeButtonCoroutine);
                _closeButtonCoroutine = null;
            }

            if (closeAdButton != null)
                closeAdButton.SetActive(false);

            if (adPlaceholderPanel != null)
                adPlaceholderPanel.SetActive(false);
        }
    }
}
