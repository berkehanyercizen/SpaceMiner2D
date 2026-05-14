// Task 2.3 — Cargo Slot UI
// Space Mining Logistics

using UnityEngine;

namespace SpaceMining
{
    public class SlotVisualizer : MonoBehaviour
    {
        public SlotManager slotManager;
        public Sprite slotSprite;

        public Vector2 slotsCenter = Vector2.zero;
        public float slotSize = 1f;
        public float slotSpacing = 1.25f;

        public Color emptySlotColor = new Color(0.25f, 0.25f, 0.3f, 1f);
        public Color fullSlotColor = new Color(0.15f, 0.15f, 0.25f, 1f);
        public Color activeSlotColor = new Color(0.4f, 0.35f, 0.1f, 1f);

        private GameObject[] slotObjects;
        private SpriteRenderer[] slotRenderers;
        private Sprite whiteSquare;

        void Start()
        {
            if (slotManager == null) slotManager = FindObjectOfType<SlotManager>();

            if (slotManager == null)
            {
                Debug.LogError("[SlotVisualizer] SlotManager reference not set and none found in scene.");
                return;
            }

            whiteSquare = CreateWhiteSquareSprite();
            BuildSlots();
        }

        void Update()
        {
            RefreshAll();
        }

        public void BuildSlots()
        {
            ClearVisuals();
            int n = slotManager.Count;
            slotObjects = new GameObject[n];
            slotRenderers = new SpriteRenderer[n];

            for (int i = 0; i < n; i++)
                slotObjects[i] = CreateSlotObject(i);
        }

        public void RefreshAll()
        {
            if (slotManager == null || slotObjects == null) return;
            if (slotObjects.Length != slotManager.Count) { BuildSlots(); return; }
            for (int i = 0; i < slotManager.Count; i++)
                RefreshSlot(i);
        }

        public void RefreshSlot(int index)
        {
            var slot = slotManager.GetSlot(index);
            if (slot == null) return;
            slotRenderers[index].color = StateColor(slot.State);
        }

        public Vector3 GetSlotWorldPosition(int index)
        {
            if (slotObjects == null || index < 0 || index >= slotObjects.Length)
                return transform.TransformPoint(SlotLocalPosition(index, slotManager != null ? slotManager.Count : 0));
            return slotObjects[index].transform.position;
        }

        public Transform GetSlotTransform(int index)
        {
            if (slotObjects == null || index < 0 || index >= slotObjects.Length) return null;
            return slotObjects[index].transform;
        }

        private Color StateColor(SlotState state)
        {
            switch (state)
            {
                case SlotState.Full: return fullSlotColor;
                case SlotState.Active: return activeSlotColor;
                default: return emptySlotColor;
            }
        }

        private GameObject CreateSlotObject(int index)
        {
            var go = new GameObject($"Slot_{index}");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = SlotLocalPosition(index, slotManager.Count);
            go.transform.localScale = new Vector3(slotSize, slotSize, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = slotSprite != null ? slotSprite : whiteSquare;
            sr.color = emptySlotColor;
            slotRenderers[index] = sr;

            return go;
        }

        private Vector3 SlotLocalPosition(int index, int n)
        {
            float offset = (index - (n - 1) * 0.5f) * slotSpacing;
            return new Vector3(slotsCenter.x + offset, slotsCenter.y, 0f);
        }

        private void ClearVisuals()
        {
            if (slotObjects == null) return;
            foreach (var go in slotObjects)
                if (go != null) Destroy(go);
            slotObjects = null;
            slotRenderers = null;
        }

        private Sprite CreateWhiteSquareSprite()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
