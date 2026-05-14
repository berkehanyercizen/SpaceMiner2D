using UnityEngine;

namespace SpaceMining
{
    [CreateAssetMenu(menuName = "SpaceMining/LevelCatalog")]
    public class LevelCatalog : ScriptableObject
    {
        public LevelData[] levels;

        public LevelData GetAt(int index)
        {
            if (levels == null || index < 0 || index >= levels.Length) return null;
            return levels[index];
        }

        public LevelData GetNext(int currentIndex)
        {
            return GetAt(currentIndex + 1);
        }

        public int Count => levels == null ? 0 : levels.Length;
    }
}
