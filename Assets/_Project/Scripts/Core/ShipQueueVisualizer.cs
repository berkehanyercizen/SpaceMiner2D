// Task 2.1 + 2.2 — Gemi Görseli & Kuyruk Sistemi
// Space Mining Logistics

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace SpaceMining
{
    public class ShipQueueVisualizer : MonoBehaviour
    {
        public GridManager gridManager;
        public GridVisualizer gridVisualizer;
        public GameObject shipPrefab;
        public TMP_FontAsset droneCountFont;
        public int visibleCount = 3;
        public float shipSize = 1f;
        public float shipSpacing = 1.5f;
        public float columnSpacing = 1.5f;
        [Tooltip("Drone sayı yazısının world-space boyutu (gemi ile orantılı tutun, örn: shipSize=1 için 0.6).")]
        public float droneCountFontSize = 0.6f;
        public int droneCountSortingOrderOffset = 10;
        public Vector2 queueCenter;
        [Tooltip("Geminin kuyrukta yukarı kayma süresi (saniye).")]
        public float slideDuration = 0.25f;
        public string idleState = "IDLE";
        private List<GameObject>[] columnObjects;

        void Start()
        {
            if (gridManager == null)
            {
                Debug.LogError("[ShipQueueVisualizer] GridManager reference not set in Inspector.");
                return;
            }
            if (shipPrefab == null)
            {
                Debug.LogError("[ShipQueueVisualizer] 'Ship Prefab' is not assigned. Drag a TinyShip prefab into the Ship Prefab field.");
                return;
            }
            columnObjects = new List<GameObject>[3];
            for (int i = 0; i < 3; i++)
                columnObjects[i] = new List<GameObject>();
            BuildAllColumns();
        }

        void OnValidate()
        {
            if (columnObjects != null && gridManager != null)
                BuildAllColumns();
        }

        public void BuildAllColumns()
        {
            for (int i = 0; i < 3; i++)
                BuildColumn(i);
        }

        public void RefreshColumn(int col)
        {
            BuildColumn(col);
        }

        public void SlideColumnUp(int col)
        {
            if (columnObjects == null || col < 0 || col >= columnObjects.Length) return;

            for (int i = 0; i < columnObjects[col].Count; i++)
            {
                var go = columnObjects[col][i];
                if (go == null) continue;
                var queueRef = go.GetComponent<ShipQueueRef>();
                if (queueRef == null)
                {
                    queueRef = go.AddComponent<ShipQueueRef>();
                    queueRef.column = col;
                }
                queueRef.row = i;
                StartCoroutine(SlideTo(go, ShipWorldPosition(col, i)));
            }

            int currentVisible = columnObjects[col].Count;
            int queueCount = gridManager.Columns[col].Count;
            int newVisibleCount = Mathf.Min(visibleCount, queueCount);
            if (newVisibleCount > currentVisible)
            {
                var newShip = gridManager.Columns[col].GetVisible(newVisibleCount).ElementAt(newVisibleCount - 1);
                int newRow = newVisibleCount - 1;
                var go = CreateShipObject(newShip, col, newRow + 1);
                columnObjects[col].Add(go);
                var queueRef = go.GetComponent<ShipQueueRef>();
                if (queueRef != null) queueRef.row = newRow;
                StartCoroutine(SlideTo(go, ShipWorldPosition(col, newRow)));
            }
        }

        private IEnumerator SlideTo(GameObject go, Vector3 targetLocalPos)
        {
            if (go == null) yield break;
            Vector3 start = go.transform.localPosition;
            float t = 0f;
            float duration = Mathf.Max(0.0001f, slideDuration);
            while (t < duration)
            {
                if (go == null) yield break;
                t += Time.deltaTime;
                float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
                go.transform.localPosition = Vector3.Lerp(start, targetLocalPos, a);
                yield return null;
            }
            if (go == null) yield break;
            go.transform.localPosition = targetLocalPos;
        }

        public void RefreshAll()
        {
            for (int i = 0; i < 3; i++)
                BuildColumn(i);
        }

        public GameObject DetachHead(int col)
        {
            if (columnObjects == null || col < 0 || col >= columnObjects.Length) return null;
            if (columnObjects[col].Count == 0) return null;
            var go = columnObjects[col][0];
            columnObjects[col].RemoveAt(0);
            var queueRef = go.GetComponent<ShipQueueRef>();
            if (queueRef != null) Destroy(queueRef);
            foreach (var c in go.GetComponentsInChildren<Collider2D>())
                Destroy(c);
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;
            return go;
        }

        private void BuildColumn(int col)
        {
            foreach (var go in columnObjects[col])
                if (go != null) Destroy(go);
            columnObjects[col].Clear();

            int row = 0;
            foreach (var ship in gridManager.Columns[col].GetVisible(visibleCount))
            {
                var go = CreateShipObject(ship, col, row);
                columnObjects[col].Add(go);
                row++;
            }
            Debug.Log($"[ShipQueueVisualizer] BuildColumn {col}: queueCount={gridManager.Columns[col].Count}, rendered={row}");
        }

        private GameObject CreateShipObject(CargoShip ship, int col, int row)
        {
            var go = Instantiate(shipPrefab);
            go.name = $"Ship_Col{col}_Row{row}";
            go.transform.SetParent(transform, false);
            go.transform.localPosition = ShipWorldPosition(col, row);
            go.transform.localScale = new Vector3(shipSize, shipSize, 1f);

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white;
                sr.sortingOrder = 1;
            }

            var anim = go.GetComponent<Animator>();
            anim?.Play(idleState);

            var tapCollider = go.AddComponent<BoxCollider2D>();
            if (sr != null && sr.sprite != null)
            {
                tapCollider.offset = sr.sprite.bounds.center;
                tapCollider.size = sr.sprite.bounds.size;
            }
            else
            {
                tapCollider.size = Vector2.one;
            }

            var queueRef = go.AddComponent<ShipQueueRef>();
            queueRef.column = col;
            queueRef.row = row;

            var textGo = new GameObject("DroneCount");
            textGo.transform.SetParent(go.transform, false);
            textGo.transform.localPosition = new Vector3(0f, 0f, -0.1f);
            textGo.transform.localScale = Vector3.one;

            var tmp = textGo.AddComponent<TextMeshPro>();
            if (droneCountFont != null)
                tmp.font = droneCountFont;
            else if (tmp.font == null)
                Debug.LogError("[ShipQueueVisualizer] TMP default font yok. Window > TextMeshPro > Import TMP Essential Resources çalıştırın veya Inspector'dan 'Drone Count Font' atayın.");

            tmp.text = ship.DronesRemaining.ToString();
            tmp.fontSize = droneCountFontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            Color oreColor = GetVisualColor(ship.color);
            tmp.outlineColor = Color.black;
            tmp.outlineWidth = 0.5f;
            tmp.color = oreColor;
            tmp.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, oreColor);
            tmp.enableAutoSizing = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.rectTransform.sizeDelta = new Vector2(shipSize * 2f, shipSize * 2f);
            tmp.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            if (sr != null)
            {
                var tmpRenderer = tmp.GetComponent<Renderer>();
                tmpRenderer.sortingLayerID = sr.sortingLayerID;
                tmpRenderer.sortingOrder = sr.sortingOrder + droneCountSortingOrderOffset;
            }

            return go;
        }

        private Vector3 ShipWorldPosition(int col, int row)
        {
            float x = queueCenter.x + (col - 1) * columnSpacing;
            float y = queueCenter.y - row * shipSpacing;
            return new Vector3(x, y, 0f);
        }

        private Color GetVisualColor(OreColor oreColor)
        {
            if (gridVisualizer == null)
            {
                Debug.LogError("[ShipQueueVisualizer] GridVisualizer reference not set in Inspector — drone count colors will fall back to white.");
                return Color.white;
            }
            return gridVisualizer.GetVisualColor(oreColor);
        }
    }
}
