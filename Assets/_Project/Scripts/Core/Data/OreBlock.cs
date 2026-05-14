// Task 1.3 — Core Data Structures
// Space Mining Logistics

using UnityEngine;

namespace SpaceMining
{
    [System.Serializable]
    public class OreBlock
    {
        public OreColor color;
        public Vector2Int gridPosition;
        public bool isAvailable;
        public bool isLocked;
        public bool isMined;
        public SpecialMarker specialMarker = SpecialMarker.None;

        public OreBlock(OreColor color, Vector2Int position)
        {
            this.color = color;
            this.gridPosition = position;
            isAvailable = false;
            isLocked = false;
            isMined = false;
        }

        public bool IsTargetable() => isAvailable && !isLocked && !isMined;

        public bool TryLock()
        {
            if (IsTargetable()) { isLocked = true; return true; }
            return false;
        }

        public void Unlock() => isLocked = false;

        public void Mine() => isMined = true;
    }
}
