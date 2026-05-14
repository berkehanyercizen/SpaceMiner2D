using System.Collections.Generic;
using UnityEngine;

namespace SpaceMining
{
    public static class GridPathfinder
    {
        private static readonly (int dx, int dy, int cost)[] Directions =
        {
            (1, 0, 10), (-1, 0, 10), (0, 1, 10), (0, -1, 10),
            (1, 1, 14), (1, -1, 14), (-1, 1, 14), (-1, -1, 14)
        };

        public static List<Vector3> FindPath(Vector3 startWorld, Vector2Int targetGrid, GridManager gm, GridVisualizer gv)
        {
            Vector2Int startGrid = gv.WorldToGridPos(startWorld);

            const int pad = 3;
            int minX = Mathf.Min(startGrid.x, targetGrid.x) - pad;
            int maxX = Mathf.Max(startGrid.x, targetGrid.x) + pad;
            int minY = Mathf.Min(startGrid.y, targetGrid.y) - pad;
            int maxY = Mathf.Max(startGrid.y, targetGrid.y) + pad;

            var open = new List<Node>();
            var closed = new HashSet<Vector2Int>();
            var nodeMap = new Dictionary<Vector2Int, Node>();

            var startNode = new Node(startGrid, 0, Heuristic(startGrid, targetGrid), startGrid);
            open.Add(startNode);
            nodeMap[startGrid] = startNode;

            while (open.Count > 0)
            {
                int bestIdx = 0;
                for (int i = 1; i < open.Count; i++)
                    if (open[i].f < open[bestIdx].f) bestIdx = i;

                var current = open[bestIdx];
                open.RemoveAt(bestIdx);

                if (current.pos == targetGrid)
                    return ReconstructPath(current.pos, nodeMap, targetGrid, gm, gv);

                closed.Add(current.pos);

                foreach (var (dx, dy, moveCost) in Directions)
                {
                    var neighbor = new Vector2Int(current.pos.x + dx, current.pos.y + dy);
                    if (neighbor.x < minX || neighbor.x > maxX || neighbor.y < minY || neighbor.y > maxY) continue;
                    if (closed.Contains(neighbor)) continue;
                    if (!IsPassable(neighbor.x, neighbor.y, targetGrid, gm)) continue;

                    int tentativeG = current.g + moveCost;
                    if (nodeMap.TryGetValue(neighbor, out var existing) && tentativeG >= existing.g) continue;

                    var node = new Node(neighbor, tentativeG, tentativeG + Heuristic(neighbor, targetGrid), current.pos);
                    nodeMap[neighbor] = node;
                    if (!open.Contains(node)) open.Add(node);
                }
            }

            return null;
        }

        private static List<Vector3> ReconstructPath(Vector2Int end, Dictionary<Vector2Int, Node> nodeMap, Vector2Int target, GridManager gm, GridVisualizer gv)
        {
            var path = new List<Vector2Int>();
            var current = end;
            while (nodeMap.TryGetValue(current, out var node) && node.pos != node.parent)
            {
                path.Add(current);
                current = node.parent;
            }
            path.Reverse();

            var result = new List<Vector3>(path.Count);
            foreach (var p in path)
                result.Add(gv.BlockWorldPosition3D(p));
            return result;
        }

        private static bool IsPassable(int x, int y, Vector2Int target, GridManager gm)
        {
            if (x < 0 || x >= gm.Width || y < 0 || y >= gm.Height) return true;
            if (x == target.x && y == target.y) return true;
            var block = gm.GetBlock(x, y);
            if (block == null) return true;
            if (block.isMined) return true;
            return false;
        }

        private static int Heuristic(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return 10 * Mathf.Max(dx, dy) + 4 * Mathf.Min(dx, dy);
        }

        private struct Node
        {
            public Vector2Int pos;
            public int g;
            public int f;
            public Vector2Int parent;

            public Node(Vector2Int pos, int g, int f, Vector2Int parent)
            {
                this.pos = pos;
                this.g = g;
                this.f = f;
                this.parent = parent;
            }
        }
    }
}
