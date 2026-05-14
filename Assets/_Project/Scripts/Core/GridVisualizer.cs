// Task 1.6 — Grid Görselleştirme
// Space Mining Logistics

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SpaceMining
{
    public class GridVisualizer : MonoBehaviour
    {
        [System.Serializable]
        public struct OreColorEntry
        {
            public OreColor oreColor;
            public Color visualColor;
        }

        public GridManager gridManager;
        public OreColorEntry[] colorPalette;
        public float cellSize = 1f;
        public Vector2 gridCenter = Vector2.zero;

        [Header("Block Visuals")]
        public float borderScale = 1.18f;
        public float reflectionAlpha = 1f;

        [Header("Block Bevel")]
        public float bevelWidth = 0.15f;
        [Range(0f, 1f)] public float bevelStrength = 0.45f;

        [Header("VFX")]
        public GameObject oreBreakPrefab;

        [Header("Bottom Shade")]
        public float shadeDepth = 0.25f;
        [Range(0f, 90f)]
        public float shadeAngle = 60f;
        [Range(0f, 1f)]
        public float shadeDarkness = 0.45f;

        private Dictionary<Vector2Int, GameObject> blockObjects = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, GameObject> shadeObjects = new Dictionary<Vector2Int, GameObject>();
        private Sprite whiteSquare;

        void Start()
        {
            if (gridManager == null)
            {
                Debug.LogError("[GridVisualizer] GridManager reference not set in Inspector.");
                return;
            }

            var ld = gridManager.levelData;
            if (ld != null)
            {
                if (ld.cellSize > 0f) cellSize = ld.cellSize;
                if (ld.colorPalette != null && ld.colorPalette.Length > 0) colorPalette = ld.colorPalette;
                if (ld.bevelWidth > 0f) bevelWidth = ld.bevelWidth;
                bevelStrength = ld.bevelStrength;
                gridCenter = ld.gridPositionOffset;
            }

            whiteSquare = CreateWhiteSquareSprite();
            BuildVisuals();
        }

        private Sprite CreateWhiteSquareSprite()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        void OnValidate()
        {
            if (!Application.isPlaying) return;
            if (blockObjects != null && gridManager != null)
            {
                gridManager.BuildGrid();
                foreach (var go in blockObjects.Values)
                    if (go != null) Destroy(go);
                blockObjects.Clear();
                shadeObjects.Clear();
                BuildVisuals();
            }
        }

        public void BuildVisuals()
        {
            // Defensive cleanup: drop any leftover Block_X_Y children that may
            // have been saved into the scene from a previous Play session.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var c = transform.GetChild(i);
                if (c != null && c.name.StartsWith("Block_"))
                    Destroy(c.gameObject);
            }

            blockObjects.Clear();
            shadeObjects.Clear();

            for (int x = 0; x < gridManager.Width; x++)
                for (int y = 0; y < gridManager.Height; y++)
                {
                    var block = gridManager.GetBlock(x, y);
                    if (block == null) continue;
                    var go = CreateBlockObject(block);
                    blockObjects[block.gridPosition] = go;
                    TryAddBottomShade(block);
                }
        }

        public void RefreshBlock(OreBlock block)
        {
            if (block == null) return;
            if (!blockObjects.TryGetValue(block.gridPosition, out GameObject go)) return;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) return;
            if (!block.isMined)
                sr.color = GetVisualColor(block.color);
        }

        public void VanishBlock(OreBlock block)
        {
            if (block == null) return;

            if (oreBreakPrefab != null)
            {
                var vfx = Instantiate(oreBreakPrefab, BlockWorldPosition3D(block.gridPosition), Quaternion.identity);
                var ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startColor = GetVisualColor(block.color);
                }
            }

            if (!blockObjects.TryGetValue(block.gridPosition, out GameObject go)) return;
            StartCoroutine(VanishCoroutine(go, block.gridPosition));
        }

        private IEnumerator VanishCoroutine(GameObject go, Vector2Int pos)
        {
            float t = 0f, duration = 0.25f;
            Vector3 startScale = go.transform.localScale;
            while (t < duration)
            {
                if (go == null) yield break;
                go.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t / duration);
                t += Time.deltaTime;
                yield return null;
            }
            if (go == null) yield break;
            go.SetActive(false);
            go.transform.localScale = startScale;

            if (shadeObjects.TryGetValue(pos, out var ownShade) && ownShade != null)
            {
                Destroy(ownShade);
                shadeObjects.Remove(pos);
            }
            blockObjects.Remove(pos);

            var blockAbove = gridManager.GetBlock(pos.x, pos.y + 1);
            if (blockAbove != null && !blockAbove.isMined)
                TryAddBottomShade(blockAbove);
        }

        public Vector3 BlockWorldPosition3D(Vector2Int gridPos)
        {
            Vector2 local = BlockWorldPosition(gridPos);
            return transform.TransformPoint(new Vector3(local.x, local.y, 0f));
        }

        public Vector2 BlockWorldPosition(Vector2Int gridPos)
        {
            Vector2 origin = gridCenter - new Vector2(
                (gridManager.Width  - 1) * cellSize * 0.5f,
                (gridManager.Height - 1) * cellSize * 0.5f
            );
            return origin + new Vector2(gridPos.x * cellSize, gridPos.y * cellSize);
        }

        private bool HasSolidBlockBelow(int x, int y)
        {
            var b = gridManager.GetBlock(x, y - 1);
            return b != null && !b.isMined;
        }

        private void TryAddBottomShade(OreBlock block)
        {
            if (!blockObjects.TryGetValue(block.gridPosition, out _)) return;
            if (shadeObjects.ContainsKey(block.gridPosition)) return;
            if (HasSolidBlockBelow(block.gridPosition.x, block.gridPosition.y)) return;
            AddBottomShade(block, GetVisualColor(block.color));
        }

        private void AddBottomShade(OreBlock block, Color blockColor)
        {
            if (!blockObjects.TryGetValue(block.gridPosition, out GameObject parent)) return;

            float angleRad = shadeAngle * Mathf.Deg2Rad;
            float xShift = shadeDepth * Mathf.Cos(angleRad);
            float yShift = shadeDepth * Mathf.Sin(angleRad);

            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f,          -0.5f,          0f),
                new Vector3( 0.5f,          -0.5f,          0f),
                new Vector3( 0.5f + xShift, -0.5f - yShift, 0f),
                new Vector3(-0.5f + xShift, -0.5f - yShift, 0f)
            };
            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Color shade = new Color(
                blockColor.r * shadeDarkness,
                blockColor.g * shadeDarkness,
                blockColor.b * shadeDarkness,
                blockColor.a);

            var child = new GameObject("BottomShade");
            child.transform.SetParent(parent.transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localScale = Vector3.one;

            child.AddComponent<MeshFilter>().mesh = mesh;
            var mr = child.AddComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = shade;
            mr.material = mat;
            mr.sortingLayerName = "Default";
            mr.sortingOrder = -2;

            shadeObjects[block.gridPosition] = child;
        }

        private GameObject CreateBlockObject(OreBlock block)
        {
            var go = new GameObject($"Block_{block.gridPosition.x}_{block.gridPosition.y}");
            go.transform.SetParent(transform, false);
            Vector2 pos = BlockWorldPosition(block.gridPosition);
            go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = new Vector3(cellSize, cellSize, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = whiteSquare;
            sr.color = GetVisualColor(block.color);

            go.AddComponent<BoxCollider2D>();

            AddBorder(go);
            AddReflection(go);
            AddBevel(go, block);

            return go;
        }

        private void AddBorder(GameObject parent)
        {
            CreateChildSprite(parent, "Border",
                Vector2.zero,
                new Vector2(borderScale, borderScale),
                Color.black, -1);
        }

        private void AddReflection(GameObject parent)
        {
            Color c = new Color(1f, 1f, 1f, reflectionAlpha);
            CreateChildSprite(parent, "ReflectionH",
                new Vector2(-0.175f, 0.29f),
                new Vector2(0.35f, 0.12f),
                c, 1);
            CreateChildSprite(parent, "ReflectionV",
                new Vector2(-0.29f, 0.175f),
                new Vector2(0.12f, 0.35f),
                c, 1);
        }

        private void AddBevel(GameObject parent, OreBlock block)
        {
            if (bevelWidth <= 0f || bevelStrength <= 0f) return;

            Color baseColor = GetVisualColor(block.color);
            Color highlight = Color.Lerp(baseColor, Color.white, bevelStrength);
            Color shadow    = Color.Lerp(baseColor, Color.black, bevelStrength);
            Color highlightFade = highlight; highlightFade.a = 0f;
            Color shadowFade    = shadow;    shadowFade.a    = 0f;

            float w = bevelWidth;

            // Top highlight: opaque at top edge, fades to transparent at inset
            AddBevelStrip(parent, "BevelTop",
                new Vector3(-0.5f, 0.5f,     0f), highlight,
                new Vector3( 0.5f, 0.5f,     0f), highlight,
                new Vector3( 0.5f, 0.5f - w, 0f), highlightFade,
                new Vector3(-0.5f, 0.5f - w, 0f), highlightFade);

            // Left highlight
            AddBevelStrip(parent, "BevelLeft",
                new Vector3(-0.5f,     0.5f,  0f), highlight,
                new Vector3(-0.5f + w, 0.5f,  0f), highlightFade,
                new Vector3(-0.5f + w, -0.5f, 0f), highlightFade,
                new Vector3(-0.5f,    -0.5f,  0f), highlight);

            // Bottom shadow
            AddBevelStrip(parent, "BevelBottom",
                new Vector3(-0.5f, -0.5f + w, 0f), shadowFade,
                new Vector3( 0.5f, -0.5f + w, 0f), shadowFade,
                new Vector3( 0.5f, -0.5f,     0f), shadow,
                new Vector3(-0.5f, -0.5f,     0f), shadow);

            // Right shadow
            AddBevelStrip(parent, "BevelRight",
                new Vector3( 0.5f - w,  0.5f, 0f), shadowFade,
                new Vector3( 0.5f,      0.5f, 0f), shadow,
                new Vector3( 0.5f,     -0.5f, 0f), shadow,
                new Vector3( 0.5f - w, -0.5f, 0f), shadowFade);
        }

        private void AddBevelStrip(GameObject parent, string name,
            Vector3 v0, Color c0, Vector3 v1, Color c1,
            Vector3 v2, Color c2, Vector3 v3, Color c3)
        {
            var mesh = new Mesh();
            mesh.vertices  = new[] { v0, v1, v2, v3 };
            mesh.colors    = new[] { c0, c1, c2, c3 };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localScale = Vector3.one;

            child.AddComponent<MeshFilter>().mesh = mesh;
            var mr = child.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Sprites/Default"));
            mr.sortingLayerName = "Default";
            mr.sortingOrder = 1;
        }

        private void CreateChildSprite(GameObject parent, string name,
            Vector2 localPos, Vector2 localScale, Color color, int sortOrder)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            child.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
            child.transform.localScale = new Vector3(localScale.x, localScale.y, 1f);
            var sr = child.AddComponent<SpriteRenderer>();
            sr.sprite = whiteSquare;
            sr.color = color;
            sr.sortingOrder = sortOrder;
        }

        public Vector2Int WorldToGridPos(Vector3 worldPos)
        {
            Vector2 local = transform.InverseTransformPoint(worldPos);
            Vector2 origin = gridCenter - new Vector2(
                (gridManager.Width  - 1) * cellSize * 0.5f,
                (gridManager.Height - 1) * cellSize * 0.5f);
            int x = Mathf.RoundToInt((local.x - origin.x) / cellSize);
            int y = Mathf.RoundToInt((local.y - origin.y) / cellSize);
            return new Vector2Int(x, y);
        }

        public Color GetVisualColor(OreColor oreColor)
        {
            foreach (var entry in colorPalette)
                if (entry.oreColor == oreColor) return entry.visualColor;
            Debug.LogWarning($"[GridVisualizer] No color defined for {oreColor}, using white.");
            return Color.white;
        }
    }
}
