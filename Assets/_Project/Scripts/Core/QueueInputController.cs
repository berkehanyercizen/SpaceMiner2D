// Task 2.4 — Tap-to-select
// Space Mining Logistics

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceMining
{
    public class QueueInputController : MonoBehaviour
    {
        public Camera inputCamera;
        public GridManager gridManager;
        public SlotManager slotManager;
        public ShipQueueVisualizer queueVisualizer;
        public SlotVisualizer slotVisualizer;
        public DroneManager droneManager;
        public GameManager gameManager;
        [Tooltip("Geminin slota gitme animasyon süresi (saniye).")]
        public float moveDuration = 0.25f;
        public string moveState = "MOVE";
        public string idleState = "IDLE";

        void Start()
        {
            Debug.Log("[QueueInput] Start() running — script is alive.");
            if (inputCamera == null) inputCamera = Camera.main;
            if (slotVisualizer == null) slotVisualizer = FindObjectOfType<SlotVisualizer>();

            if (inputCamera == null) Debug.LogError("[QueueInputController] No input camera and Camera.main is null. Tag your camera as 'MainCamera' or assign it manually.");
            if (gridManager == null) Debug.LogError("[QueueInputController] GridManager reference not set in Inspector.");
            if (slotManager == null) Debug.LogError("[QueueInputController] SlotManager reference not set in Inspector.");
            if (queueVisualizer == null) Debug.LogError("[QueueInputController] ShipQueueVisualizer reference not set in Inspector.");
            if (slotVisualizer == null) Debug.LogError("[QueueInputController] SlotVisualizer reference not set in Inspector.");

            Debug.Log($"[QueueInput] Mouse.current={(Mouse.current != null ? "OK" : "NULL")}, Touchscreen.current={(Touchscreen.current != null ? "OK" : "NULL")}");
        }

        void Update()
        {
            if (gameManager != null && gameManager.IsOverlayActive) return;
            if (!TryGetTapPosition(out Vector2 screenPos)) return;
            Debug.Log($"[QueueInput] Tap detected at screen={screenPos}");

            if (inputCamera == null || gridManager == null || slotManager == null || queueVisualizer == null || slotVisualizer == null)
            {
                Debug.LogError($"[QueueInput] Missing reference — inputCamera={inputCamera!=null}, gridManager={gridManager!=null}, slotManager={slotManager!=null}, queueVisualizer={queueVisualizer!=null}, slotVisualizer={slotVisualizer!=null}");
                return;
            }

            Vector3 sp = new Vector3(screenPos.x, screenPos.y, -inputCamera.transform.position.z);
            Vector3 worldPos = inputCamera.ScreenToWorldPoint(sp);
            Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

            Collider2D hit = Physics2D.OverlapPoint(worldPos2D);
            if (hit == null) return;

            var shipRef = hit.GetComponent<ShipQueueRef>();
            if (shipRef == null) return;
            if (shipRef.row != 0) return;

            TryPlaceHeadShip(shipRef.column);
        }

        private bool TryGetTapPosition(out Vector2 screenPos)
        {
            screenPos = default;
            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            {
                screenPos = touch.primaryTouch.position.ReadValue();
                return true;
            }
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPos = mouse.position.ReadValue();
                return true;
            }
            return false;
        }

        private void TryPlaceHeadShip(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= gridManager.Columns.Count) return;
            var column = gridManager.Columns[columnIndex];
            if (column.PeekHead() == null) return;

            int slotIndex = slotManager.FindFirstEmpty();
            if (slotIndex < 0)
            {
                Debug.Log("[QueueInput] No empty slot available.");
                return;
            }

            var head = column.TakeHead();
            slotManager.TryPlaceShip(slotIndex, head);
            gameManager?.EvaluateState();

            var movingGo = queueVisualizer.DetachHead(columnIndex);
            queueVisualizer.SlideColumnUp(columnIndex);

            if (movingGo == null) return;

            var slotTransform = slotVisualizer.GetSlotTransform(slotIndex);
            Vector3 targetWorld = slotVisualizer.GetSlotWorldPosition(slotIndex);
            StartCoroutine(MoveShipToSlot(movingGo, targetWorld, slotTransform, head, slotIndex));
        }

        private IEnumerator MoveShipToSlot(GameObject go, Vector3 targetWorld, Transform targetParent, CargoShip ship, int slotIndex)
        {
            var animator = go != null ? go.GetComponent<Animator>() : null;
            animator?.Play(moveState);

            Vector3 start = go.transform.position;
            float t = 0f;
            float duration = Mathf.Max(0.0001f, moveDuration);
            while (t < duration)
            {
                if (go == null) yield break;
                t += Time.deltaTime;
                float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
                go.transform.position = Vector3.Lerp(start, targetWorld, a);
                yield return null;
            }
            if (go == null) yield break;
            go.transform.position = targetWorld;
            if (targetParent != null) go.transform.SetParent(targetParent, true);
            animator?.Play(idleState);
            droneManager?.RegisterShip(ship, targetWorld, slotIndex, go);
        }
    }
}
