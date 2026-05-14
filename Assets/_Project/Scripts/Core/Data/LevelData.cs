// Task 3.6 — Level Data Format
// Space Mining Logistics

using UnityEngine;

namespace SpaceMining
{
    [CreateAssetMenu(menuName = "SpaceMining/LevelData")]
    public class LevelData : ScriptableObject
    {
        public int gridWidth = 6;
        public int gridHeight = 4;
        public int slotCount = 4;
        public int visibleQueueRows = 3;
        [TextArea(4, 10)] public string typeLayout;
        [TextArea(4, 10)] public string availabilityLayout;
        [TextArea(3, 6)]  public string shipQueueLayout;
        public bool isTutorialLevel;

        [Header("Visuals (per level)")]
        public float cellSize = 1f;
        public GridVisualizer.OreColorEntry[] colorPalette;
        [Range(0f, 0.3f)] public float bevelWidth = 0.15f;
        [Range(0f, 1f)]   public float bevelStrength = 0.45f;

        [Tooltip("World-space offset for the grid center. (0,0) = screen center. " +
                 "Use this to nudge the grid up/down/left/right per level so it does " +
                 "not overlap HUD or ship queue.")]
        public Vector2 gridPositionOffset = Vector2.zero;

        [Header("Reward Mechanic")]
        public bool enableChestReward = true;
        public bool forceBuffOnly = false;
        public bool forceNerfOnly = false;

        [Tooltip("Upper bound on grid Y for chest/key spawn. -1 = no constraint (anywhere). " +
                 "Set to N to confine spawns to the bottom N+1 rows (y in [0, N]) so the " +
                 "player encounters the reward earlier.")]
        public int chestSpawnYMax = -1;
    }
}
