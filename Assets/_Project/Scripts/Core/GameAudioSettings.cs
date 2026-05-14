using UnityEngine;

namespace SpaceMining
{
    public static class GameAudioSettings
    {
        private const string MusicKey = "music_on";
        private const string SfxKey = "sfx_on";
        private const string HapticsKey = "haptics_on";

        public static event System.Action<bool> OnMusicChanged;
        public static event System.Action<bool> OnSfxChanged;
        public static event System.Action<bool> OnHapticsChanged;

        public static bool IsMusicOn() => PlayerPrefs.GetInt(MusicKey, 1) == 1;
        public static bool IsSfxOn() => PlayerPrefs.GetInt(SfxKey, 1) == 1;
        public static bool IsHapticsOn() => PlayerPrefs.GetInt(HapticsKey, 1) == 1;

        public static void SetMusicOn(bool on)
        {
            PlayerPrefs.SetInt(MusicKey, on ? 1 : 0);
            PlayerPrefs.Save();
            OnMusicChanged?.Invoke(on);
        }

        public static void SetSfxOn(bool on)
        {
            PlayerPrefs.SetInt(SfxKey, on ? 1 : 0);
            PlayerPrefs.Save();
            OnSfxChanged?.Invoke(on);
        }

        public static void SetHapticsOn(bool on)
        {
            PlayerPrefs.SetInt(HapticsKey, on ? 1 : 0);
            PlayerPrefs.Save();
            OnHapticsChanged?.Invoke(on);
        }
    }
}
