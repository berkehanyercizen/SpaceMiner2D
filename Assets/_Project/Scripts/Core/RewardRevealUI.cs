using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceMining
{
    public class RewardRevealUI : MonoBehaviour
    {
        public GameObject panel;
        public Image revealImage;
        public TMP_Text labelText;
        public float autoDismissSeconds = 2f;

        private Coroutine _activeCoroutine;
        private System.Action _onDismissed;
        private bool _isShowing;
        private GameManager _gameManager;

        private void Awake() => _gameManager = FindObjectOfType<GameManager>();

        public void ShowReveal(Sprite sprite, string label, System.Action onDismissed)
        {
            if (revealImage != null) revealImage.sprite = sprite;
            if (labelText != null) labelText.text = label;
            if (panel != null) panel.SetActive(true);

            _gameManager?.RequestPause();
            AudioManager.PlayRewardReveal();
            _onDismissed = onDismissed;
            _isShowing = true;

            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
            _activeCoroutine = StartCoroutine(AutoDismissRoutine());
        }

        public void OnTap()
        {
            if (_isShowing) Dismiss();
        }

        private void Update()
        {
            if (!_isShowing) return;

            if (Input.GetMouseButtonDown(0))
            {
                Dismiss();
                return;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    Dismiss();
                    return;
                }
            }
        }

        private IEnumerator AutoDismissRoutine()
        {
            yield return new WaitForSecondsRealtime(autoDismissSeconds);
            Dismiss();
        }

        private void Dismiss()
        {
            if (!_isShowing) return;
            _isShowing = false;

            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
                _activeCoroutine = null;
            }

            if (panel != null) panel.SetActive(false);
            _gameManager?.ReleasePause();

            var callback = _onDismissed;
            _onDismissed = null;
            callback?.Invoke();
        }
    }
}
