using UnityEngine;

namespace SpaceMining
{
    public class HapticManager : MonoBehaviour
    {
        public static HapticManager Instance { get; private set; }

        private const long OreMinedMs = 25;
        private const long DroneDispatchedMs = 60;

#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject vibrator;
        private int androidApiLevel;
#endif

        public static void PlayOreMined()        { Instance?.Trigger(OreMinedMs); }
        public static void PlayDroneDispatched() { Instance?.Trigger(DroneDispatchedMs); }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID && !UNITY_EDITOR
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                androidApiLevel = version.GetStatic<int>("SDK_INT");
            }
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            }
#endif
        }

        private void Trigger(long ms)
        {
            if (!GameAudioSettings.IsHapticsOn()) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            if (vibrator == null) return;
            if (androidApiLevel >= 26)
            {
                using (var effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                using (var effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", ms, -1))
                {
                    vibrator.Call("vibrate", effect);
                }
            }
            else
            {
                vibrator.Call("vibrate", ms);
            }
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
