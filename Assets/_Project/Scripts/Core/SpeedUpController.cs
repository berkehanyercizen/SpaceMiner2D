using UnityEngine;

namespace SpaceMining
{
    public class SpeedUpController : MonoBehaviour
    {
        public GameManager gameManager;
        public GameObject tapArea;
        public GameObject speedIndicator;
        public GameObject tapHint;

        void Update()
        {
            bool queueEmpty = gameManager != null && gameManager.IsQueueEmpty();
            bool overlayUp = gameManager != null && gameManager.IsOverlayActive;
            bool isSpeedingUp = Time.timeScale >= 2f;
            bool active = queueEmpty && Time.timeScale > 0f && !overlayUp;

            if (tapArea != null)        tapArea.SetActive(active);
            if (tapHint != null)        tapHint.SetActive(active && !isSpeedingUp);
            if (speedIndicator != null) speedIndicator.SetActive(active && isSpeedingUp);

            if (!queueEmpty || overlayUp) ResetSpeed();
        }

        public void OnTap()
        {
            if (Time.timeScale == 0f) return;
            if (gameManager != null && gameManager.IsOverlayActive) return;
            Time.timeScale = Time.timeScale >= 2f ? 1f : 2f;
        }

        private void ResetSpeed()
        {
            if (Time.timeScale >= 2f) Time.timeScale = 1f;
        }

        void OnDisable() => ResetSpeed();
    }
}
