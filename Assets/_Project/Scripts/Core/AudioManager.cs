using UnityEngine;

namespace SpaceMining
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public AudioSource musicSource;
        public AudioSource sfxSource;

        [Header("SFX Clips — assign in Inspector")]
        public AudioClip sfxShipPlaced;
        public AudioClip sfxDroneDispatched;
        public AudioClip sfxOreMined;
        public AudioClip sfxLevelClear;
        public AudioClip sfxGameOver;
        public AudioClip sfxRewardReveal;

        public static void PlayShipPlaced()     { var am = Instance; am?.PlaySfx(am.sfxShipPlaced); }
        public static void PlayDroneDispatched(){ var am = Instance; am?.PlaySfx(am.sfxDroneDispatched); }
        public static void PlayOreMined()       { var am = Instance; am?.PlaySfx(am.sfxOreMined); }
        public static void PlayLevelClear()     { var am = Instance; am?.PlaySfx(am.sfxLevelClear); }
        public static void PlayGameOver()       { var am = Instance; am?.PlaySfx(am.sfxGameOver); }
        public static void PlayRewardReveal()   { var am = Instance; am?.PlaySfx(am.sfxRewardReveal); }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            ApplyMusicState(GameAudioSettings.IsMusicOn());
            ApplySfxState(GameAudioSettings.IsSfxOn());

            GameAudioSettings.OnMusicChanged += ApplyMusicState;
            GameAudioSettings.OnSfxChanged += ApplySfxState;
        }

        void OnDestroy()
        {
            GameAudioSettings.OnMusicChanged -= ApplyMusicState;
            GameAudioSettings.OnSfxChanged -= ApplySfxState;
        }

        private void ApplyMusicState(bool on)
        {
            if (musicSource == null) return;
            musicSource.mute = !on;
            if (on && !musicSource.isPlaying) musicSource.Play();
        }

        private void ApplySfxState(bool on)
        {
            if (sfxSource == null) return;
            sfxSource.mute = !on;
        }

        public void PlaySfx(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            if (!GameAudioSettings.IsSfxOn()) return;
            sfxSource.PlayOneShot(clip);
        }
    }
}
