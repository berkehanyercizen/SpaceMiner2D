using UnityEngine;

namespace SpaceMining
{
    public static class LevelSelection
    {
        public static LevelData Current;
        public static int CurrentIndex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Current = null;
            CurrentIndex = 0;
        }
    }
}
