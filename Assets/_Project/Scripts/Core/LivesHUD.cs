using TMPro;
using UnityEngine;

namespace SpaceMining
{
    public class LivesHUD : MonoBehaviour
    {
        public TMP_Text countLabel;
        [Tooltip("Use {0} as a placeholder for the lives number.")]
        public string format = "x {0}";

        private void OnEnable()
        {
            LivesManager.OnLivesChanged += Refresh;
            Refresh(LivesManager.CurrentLives);
        }

        private void OnDisable()
        {
            LivesManager.OnLivesChanged -= Refresh;
        }

        private void Refresh(int lives)
        {
            if (countLabel != null)
                countLabel.text = string.Format(format, lives);
        }
    }
}
