using UnityEngine;
using UnityEngine.UI;

namespace SpaceMining
{
    public class SettingsToggleButton : MonoBehaviour
    {
        public enum ToggleType { Music, Sfx }

        public ToggleType type;
        public Sprite onSprite;
        public Sprite offSprite;
        public Image targetImage;

        void Awake()
        {
            UpdateSprite(GetCurrentState());
        }

        public void OnClick()
        {
            bool newState = !GetCurrentState();
            if (type == ToggleType.Music) GameAudioSettings.SetMusicOn(newState);
            else GameAudioSettings.SetSfxOn(newState);
            UpdateSprite(newState);
        }

        private bool GetCurrentState()
        {
            return type == ToggleType.Music
                ? GameAudioSettings.IsMusicOn()
                : GameAudioSettings.IsSfxOn();
        }

        private void UpdateSprite(bool on)
        {
            if (targetImage != null) targetImage.sprite = on ? onSprite : offSprite;
        }
    }
}
