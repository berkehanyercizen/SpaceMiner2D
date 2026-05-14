// Task 2.3 — Cargo Slot UI
// Space Mining Logistics

namespace SpaceMining
{
    public enum SlotState
    {
        Empty,
        Full,
        Active
    }

    public class CargoSlot
    {
        public int Index { get; }
        public CargoShip Ship { get; private set; }
        public SlotState State { get; private set; }

        public bool IsEmpty => Ship == null;
        public bool IsFull => Ship != null;

        public CargoSlot(int index)
        {
            Index = index;
            Ship = null;
            State = SlotState.Empty;
        }

        public bool TryPlaceShip(CargoShip ship)
        {
            if (ship == null || Ship != null) return false;
            Ship = ship;
            State = SlotState.Full;
            return true;
        }

        public CargoShip Clear()
        {
            var removed = Ship;
            Ship = null;
            State = SlotState.Empty;
            return removed;
        }

        public void SetActive(bool active)
        {
            if (Ship == null) { State = SlotState.Empty; return; }
            State = active ? SlotState.Active : SlotState.Full;
        }
    }
}
