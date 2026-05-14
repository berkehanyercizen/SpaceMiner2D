using UnityEngine;

namespace SpaceMining
{
    public static class LivesManager
    {
        public const int MaxLives = 5;

        public static int CurrentLives { get; private set; } = MaxLives;

        public static event System.Action<int> OnLivesChanged;

        public static void ConsumeLife()
        {
            if (CurrentLives <= 0) return;
            CurrentLives--;
            OnLivesChanged?.Invoke(CurrentLives);
        }

        public static void ResetLives()
        {
            CurrentLives = MaxLives;
            OnLivesChanged?.Invoke(CurrentLives);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            CurrentLives = MaxLives;
            OnLivesChanged = null;
        }
    }
}
