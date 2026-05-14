// Task 3.7 — Tutorial Overlay
// Space Mining Logistics

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace SpaceMining
{
    public class TutorialController : MonoBehaviour
    {
        public LevelData levelData;
        public GameObject tutorialPanel;
        public TMP_Text messageText;
        [TextArea(2, 4)] public string[] steps;
        public GameObject[] showOnComplete;

        private int _stepIndex;
        private bool _active;

        void Start()
        {
            if (LevelSelection.Current != null)
                levelData = LevelSelection.Current;

            bool isTutorial = levelData != null && levelData.isTutorialLevel;

            if (tutorialPanel != null) tutorialPanel.SetActive(isTutorial);

            if (!isTutorial)
            {
                foreach (var go in showOnComplete)
                    if (go != null) go.SetActive(true);
                return;
            }

            _stepIndex = 0;
            _active = true;
            ShowStep();
        }

        void Update()
        {
            if (!_active) return;
            if (!TappedThisFrame()) return;

            _stepIndex++;
            if (_stepIndex >= steps.Length)
            {
                _active = false;
                foreach (var go in showOnComplete)
                    if (go != null) go.SetActive(true);
                tutorialPanel.SetActive(false);
                return;
            }
            ShowStep();
        }

        private void ShowStep()
        {
            if (messageText != null && _stepIndex < steps.Length)
                messageText.text = steps[_stepIndex];
        }

        private bool TappedThisFrame()
        {
            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame) return true;
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;
            return false;
        }
    }
}
