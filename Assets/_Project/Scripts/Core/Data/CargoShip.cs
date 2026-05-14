// Task 1.3 — Core Data Structures
// Space Mining Logistics

using UnityEngine;

namespace SpaceMining
{
    [System.Serializable]
    public class CargoShip
    {
        public OreColor color;
        public int miningPower;
        public int DronesRemaining { get; private set; }
        public bool IsDepleted => DronesRemaining == 0;

        public CargoShip(OreColor color, int miningPower)
        {
            this.color = color;
            this.miningPower = miningPower;
            DronesRemaining = miningPower;
        }

        public bool TryDispatchDrone()
        {
            if (DronesRemaining > 0) { DronesRemaining--; return true; }
            return false;
        }
    }
}
