// Task 2.3 — Cargo Slot UI
// Space Mining Logistics

using UnityEngine;
using System.Collections.Generic;

namespace SpaceMining
{
    public class SlotManager : MonoBehaviour
    {
        public LevelData levelData;
        [Range(1, 8)] public int slotCount = 4;
        private CargoSlot[] slots;
        private int _pendingSlotRemovals;

        public IReadOnlyList<CargoSlot> Slots => slots;
        public int Count => slots != null ? slots.Length : 0;
        public int PendingSlotRemovals => _pendingSlotRemovals;

        void Awake()
        {
            if (levelData != null) slotCount = levelData.slotCount;
            BuildSlots();
        }

        public void BuildSlots()
        {
            slots = new CargoSlot[slotCount];
            for (int i = 0; i < slotCount; i++)
                slots[i] = new CargoSlot(i);
        }

        public CargoSlot GetSlot(int index)
        {
            if (slots == null || index < 0 || index >= slots.Length) return null;
            return slots[index];
        }

        public bool TryPlaceShip(int slotIndex, CargoShip ship)
        {
            var slot = GetSlot(slotIndex);
            if (slot == null) return false;
            bool placed = slot.TryPlaceShip(ship);
            if (placed) AudioManager.PlayShipPlaced();
            return placed;
        }

        public int FindFirstEmpty()
        {
            if (slots == null) return -1;
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].IsEmpty) return i;
            return -1;
        }

        public void ClearSlot(int index)
        {
            GetSlot(index)?.Clear();
            ProcessPendingRemovals();
        }

        public bool AreAllSlotsFull()
        {
            if (slots == null) return false;
            foreach (var s in slots)
                if (s.IsEmpty) return false;
            return true;
        }

        public void ChangeSlotCount(int delta)
        {
            if (delta == 0) return;

            if (delta < 0)
            {
                int removalsRequested = -delta;
                for (int i = 0; i < removalsRequested; i++)
                {
                    if (!TryTrimLastEmptySlot())
                    {
                        _pendingSlotRemovals++;
                        Debug.Log($"[SlotManager] Slot removal deferred — last slot occupied. Pending: {_pendingSlotRemovals}");
                    }
                }
                return;
            }

            int newCount = slotCount + delta;
            ResizeSlots(newCount);
        }

        public void ProcessPendingRemovals()
        {
            while (_pendingSlotRemovals > 0 && TryTrimLastEmptySlot())
                _pendingSlotRemovals--;
        }

        private bool TryTrimLastEmptySlot()
        {
            if (slots == null || slots.Length <= 1) return false;
            int lastIndex = slots.Length - 1;
            if (!slots[lastIndex].IsEmpty) return false;
            ResizeSlots(slots.Length - 1);
            return true;
        }

        private void ResizeSlots(int newCount)
        {
            if (newCount == slotCount) return;
            newCount = Mathf.Max(1, newCount);

            var newSlots = new CargoSlot[newCount];
            int copyCount = slots != null ? Mathf.Min(slots.Length, newCount) : 0;

            for (int i = 0; i < copyCount; i++)
                newSlots[i] = slots[i];

            for (int i = copyCount; i < newCount; i++)
                newSlots[i] = new CargoSlot(i);

            slotCount = newCount;
            slots = newSlots;
        }
    }
}
