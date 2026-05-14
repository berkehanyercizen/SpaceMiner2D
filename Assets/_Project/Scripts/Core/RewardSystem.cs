using System.Collections.Generic;
using UnityEngine;

namespace SpaceMining
{
    public class RewardSystem : MonoBehaviour
    {
        [Header("References")]
        public LevelData levelData;
        public GridManager gridManager;
        public SlotManager slotManager;
        public DroneManager droneManager;
        public RewardRevealUI revealUI;
        public RewardTrackerUI trackerUI;

        [Header("Sprites")]
        public Sprite chestSprite;
        public Sprite keySprite;
        public Sprite blueBookSprite;
        public Sprite redBookSprite;

        [Header("Modifier Pools")]
        public List<Modifier> buffs;
        public List<Modifier> nerfs;

        private OreBlock _chestBlock;
        private OreBlock _keyBlock;
        private bool _chestFound;
        private bool _keyFound;
        private bool _bookGranted;

        void Start()
        {
            if (gridManager != null && gridManager.levelData != null)
                levelData = gridManager.levelData;

            if (levelData != null && !levelData.enableChestReward)
            {
                if (trackerUI != null) trackerUI.gameObject.SetActive(false);
                return;
            }
            SelectSpecialBlocks();
        }

        private void SelectSpecialBlocks()
        {
            if (gridManager == null) return;
            var allBlocks = gridManager.GetAllBlocks();

            List<OreBlock> pool = allBlocks;
            if (levelData != null && levelData.chestSpawnYMax >= 0)
            {
                pool = new List<OreBlock>();
                foreach (var b in allBlocks)
                    if (b.gridPosition.y <= levelData.chestSpawnYMax) pool.Add(b);

                if (pool.Count < 2)
                {
                    Debug.LogWarning($"[RewardSystem] chestSpawnYMax={levelData.chestSpawnYMax} produced {pool.Count} candidate blocks; falling back to whole grid");
                    pool = allBlocks;
                }
            }

            if (pool.Count < 2)
            {
                Debug.LogWarning("[RewardSystem] Not enough blocks to assign chest+key markers");
                return;
            }

            int i = Random.Range(0, pool.Count);
            int j;
            do { j = Random.Range(0, pool.Count); } while (j == i);

            _chestBlock = pool[i];
            _keyBlock = pool[j];
            _chestBlock.specialMarker = SpecialMarker.Chest;
            _keyBlock.specialMarker = SpecialMarker.Key;

            Debug.Log($"[RewardSystem] Chest at {_chestBlock.gridPosition}, Key at {_keyBlock.gridPosition}");
        }

        public void OnSpecialMined(OreBlock block)
        {
            if (block == null) return;

            if (block.specialMarker == SpecialMarker.Chest && !_chestFound)
            {
                _chestFound = true;
                trackerUI?.MarkChestFound();
                revealUI?.ShowReveal(chestSprite, "Chest Found", CheckBothFound);
            }
            else if (block.specialMarker == SpecialMarker.Key && !_keyFound)
            {
                _keyFound = true;
                trackerUI?.MarkKeyFound();
                revealUI?.ShowReveal(keySprite, "Key Found", CheckBothFound);
            }
        }

        private void CheckBothFound()
        {
            if (_bookGranted) return;
            if (!_chestFound || !_keyFound) return;

            _bookGranted = true;

            bool isBuff;
            if (levelData != null && levelData.forceBuffOnly) isBuff = true;
            else if (levelData != null && levelData.forceNerfOnly) isBuff = false;
            else isBuff = Random.value < 0.5f;

            var pool = isBuff ? buffs : nerfs;
            if (pool == null || pool.Count == 0)
            {
                Debug.LogWarning($"[RewardSystem] {(isBuff ? "Buff" : "Nerf")} pool is empty");
                return;
            }

            var modifier = pool[Random.Range(0, pool.Count)];
            ApplyModifier(modifier);

            var bookSprite = isBuff ? blueBookSprite : redBookSprite;
            var capturedSprite = bookSprite;
            var capturedLabel = modifier.label;
            revealUI?.ShowReveal(bookSprite, modifier.label,
                () => trackerUI?.ShowModifier(capturedSprite, capturedLabel));
        }

        private void ApplyModifier(Modifier m)
        {
            if (m == null) return;

            switch (m.type)
            {
                case ModifierType.SlotCount:
                    if (slotManager != null) slotManager.ChangeSlotCount((int)m.magnitude);
                    break;
                case ModifierType.DroneSpeed:
                    if (droneManager != null) droneManager.droneSpeed *= (1f + m.magnitude);
                    break;
                case ModifierType.AttackDuration:
                    if (droneManager != null) droneManager.droneAttackDuration *= (1f + m.magnitude);
                    break;
                case ModifierType.SpawnInterval:
                    if (droneManager != null) droneManager.spawnInterval *= (1f + m.magnitude);
                    break;
            }

            Debug.Log($"[RewardSystem] Applied modifier: {m.label}");
        }
    }
}
