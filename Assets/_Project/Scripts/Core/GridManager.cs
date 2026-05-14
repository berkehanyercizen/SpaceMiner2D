// Task 1.4 — Grid & Ship Queue
// Space Mining Logistics

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SpaceMining
{
    public class GridManager : MonoBehaviour
    {
        public LevelData levelData;
        public GameManager gameManager;
        [SerializeField] private RewardSystem rewardSystem;
        public int gridWidth = 6;
        public int gridHeight = 4;

        [TextArea(4, 15)] public string typeLayout;
        [TextArea(4, 15)] public string availabilityLayout;
        [TextArea(3, 10)] public string shipQueueLayout;

        private OreBlock[,] grid;
        private ShipColumn[] columns;

        public int Width => gridWidth;
        public int Height => gridHeight;
        public IReadOnlyList<ShipColumn> Columns => columns;

        void Awake()
        {
            if (LevelSelection.Current != null)
                levelData = LevelSelection.Current;

            if (levelData != null)
            {
                gridWidth          = levelData.gridWidth;
                gridHeight         = levelData.gridHeight;
                typeLayout         = levelData.typeLayout;
                availabilityLayout = levelData.availabilityLayout;
                shipQueueLayout    = levelData.shipQueueLayout;
            }

            BuildGrid();
            BuildShipQueue();

            var oreCounts = GetUnminedOreCountByType();
            var shipPower = GetTotalShipPowerByType();

            foreach (var kvp in oreCounts)
            {
                int power = shipPower.TryGetValue(kvp.Key, out int p) ? p : 0;
                if (kvp.Value != power)
                    Debug.LogWarning($"[GridManager] BALANCE WARNING: {kvp.Key} has {kvp.Value} ore but ship power total is {power}");
            }
            foreach (var kvp in shipPower)
            {
                if (!oreCounts.ContainsKey(kvp.Key))
                    Debug.LogWarning($"[GridManager] BALANCE WARNING: {kvp.Key} has 0 ore but ship power total is {kvp.Value}");
            }

            Debug.Log($"[GridManager] Grid built: {gridWidth}x{gridHeight}");
            Debug.Log("[GridManager] Ore counts: " + DictToString(oreCounts));
            Debug.Log($"[GridManager] Columns built: [{columns[0].Count}, {columns[1].Count}, {columns[2].Count}] ships");
            Debug.Log("[GridManager] Ship power totals: " + DictToString(shipPower));
        }

        void Start() { }

        public void BuildGrid()
        {
            grid = ParseGridLayout(typeLayout, availabilityLayout);
        }

        public void BuildShipQueue()
        {
            columns = ParseShipQueue(shipQueueLayout);
        }

        public OreBlock GetBlock(int x, int y)
        {
            if (grid == null || x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return null;
            return grid[x, y];
        }

        public List<OreBlock> GetNeighbors(int x, int y)
        {
            var result = new List<OreBlock>(4);
            OreBlock b;
            if ((b = GetBlock(x, y + 1)) != null) result.Add(b);
            if ((b = GetBlock(x, y - 1)) != null) result.Add(b);
            if ((b = GetBlock(x - 1, y)) != null) result.Add(b);
            if ((b = GetBlock(x + 1, y)) != null) result.Add(b);
            return result;
        }

        public List<OreBlock> GetAllBlocks()
        {
            var result = new List<OreBlock>();
            if (grid == null) return result;
            for (int x = 0; x < gridWidth; x++)
                for (int y = 0; y < gridHeight; y++)
                    if (grid[x, y] != null) result.Add(grid[x, y]);
            return result;
        }

        public List<OreBlock> GetTargetableBlocksOfType(OreColor type)
        {
            var result = new List<OreBlock>();
            for (int x = 0; x < gridWidth; x++)
                for (int y = 0; y < gridHeight; y++)
                {
                    var block = grid[x, y];
                    if (block != null && block.color == type && block.IsTargetable())
                        result.Add(block);
                }
            return result;
        }

        public Dictionary<OreColor, int> GetUnminedOreCountByType()
        {
            var result = new Dictionary<OreColor, int>();
            for (int x = 0; x < gridWidth; x++)
                for (int y = 0; y < gridHeight; y++)
                {
                    var block = grid[x, y];
                    if (block != null && !block.isMined)
                    {
                        if (!result.ContainsKey(block.color)) result[block.color] = 0;
                        result[block.color]++;
                    }
                }
            return result;
        }

        public Dictionary<OreColor, int> GetTotalShipPowerByType()
        {
            var result = new Dictionary<OreColor, int>();
            foreach (var col in columns)
                foreach (var ship in col.GetVisible(col.Count))
                {
                    if (!result.ContainsKey(ship.color)) result[ship.color] = 0;
                    result[ship.color] += ship.miningPower;
                }
            return result;
        }

        public void OnBlockMined(OreBlock block)
        {
            if (block == null || block.isMined) return;
            block.Mine();
            AudioManager.PlayOreMined();
            HapticManager.PlayOreMined();
            foreach (var neighbor in GetNeighbors(block.gridPosition.x, block.gridPosition.y))
            {
                if (neighbor.isMined || neighbor.isAvailable) continue;
                neighbor.isAvailable = true;
            }
            if (rewardSystem != null && block.specialMarker != SpecialMarker.None)
                rewardSystem.OnSpecialMined(block);
            gameManager?.CheckOreMined();
        }

        private OreBlock[,] ParseGridLayout(string typeStr, string availStr)
        {
            var result = new OreBlock[gridWidth, gridHeight];
            var typeLines = SplitAndTrimLines(typeStr);
            var availLines = SplitAndTrimLines(availStr);

            if (typeLines.Length != gridHeight)
            {
                Debug.LogError($"[GridManager] Type layout row count mismatch: expected {gridHeight}, got {typeLines.Length}");
                return result;
            }
            if (availLines.Length != gridHeight)
            {
                Debug.LogError($"[GridManager] Availability layout row count mismatch: expected {gridHeight}, got {availLines.Length}");
                return result;
            }

            for (int row = 0; row < gridHeight; row++)
            {
                int y = gridHeight - 1 - row;
                string typeLine = typeLines[row];
                string availLine = availLines[row];

                if (typeLine.Length != gridWidth)
                {
                    Debug.LogError($"[GridManager] Type layout row {row}: expected {gridWidth} chars, got {typeLine.Length}");
                    return result;
                }
                if (availLine.Length != gridWidth)
                {
                    Debug.LogError($"[GridManager] Availability layout row {row}: expected {gridWidth} chars, got {availLine.Length}");
                    return result;
                }

                for (int x = 0; x < gridWidth; x++)
                {
                    char tc = typeLine[x];
                    char ac = availLine[x];

                    if ((tc == '.') != (ac == '.'))
                    {
                        Debug.LogError($"[GridManager] Layout mismatch at row {row}, col {x}: type='{tc}', avail='{ac}' (dot positions must match)");
                        return result;
                    }

                    if (tc == '.') continue;

                    var block = new OreBlock(CharToOreColor(tc), new Vector2Int(x, y));
                    block.isAvailable = ac == '1';
                    result[x, y] = block;
                }
            }
            return result;
        }

        private ShipColumn[] ParseShipQueue(string queueStr)
        {
            var result = new ShipColumn[] { new ShipColumn(), new ShipColumn(), new ShipColumn() };
            var lines = SplitAndTrimLines(queueStr);

            if (lines.Length != 3)
            {
                Debug.LogError($"[GridManager] Ship queue must have exactly 3 columns (rows), got {lines.Length}");
                return result;
            }

            for (int col = 0; col < 3; col++)
            {
                var parts = lines[col].Split(',');
                for (int i = 0; i < parts.Length; i++)
                {
                    string entry = parts[i].Trim();
                    if (string.IsNullOrEmpty(entry)) continue;

                    var tokens = entry.Split(':');
                    int typeNum = 0;
                    int power = 0;
                    bool valid = tokens.Length == 2
                        && int.TryParse(tokens[0].Trim(), out typeNum)
                        && int.TryParse(tokens[1].Trim(), out power)
                        && typeNum >= 1 && typeNum <= 5
                        && power > 0;

                    if (!valid)
                    {
                        Debug.LogError($"[GridManager] Invalid ship format '{entry}' at column {col}, ship {i}");
                        continue;
                    }

                    result[col].Enqueue(new CargoShip(CharToOreColor((char)('0' + typeNum)), power));
                }
            }
            return result;
        }

        private OreColor CharToOreColor(char c)
        {
            switch (c)
            {
                case '1': return OreColor.Color1;
                case '2': return OreColor.Color2;
                case '3': return OreColor.Color3;
                case '4': return OreColor.Color4;
                case '5': return OreColor.Color5;
                default:
                    Debug.LogError($"[GridManager] Unknown ore char '{c}'");
                    return OreColor.Color1;
            }
        }

        private string[] SplitAndTrimLines(string s)
        {
            if (string.IsNullOrEmpty(s)) return new string[0];
            return s.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToArray();
        }

        private string DictToString<TKey>(Dictionary<TKey, int> dict)
        {
            return string.Join(", ", dict.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        }
    }
}
