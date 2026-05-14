// Task 1.4 — Grid & Ship Queue
// Space Mining Logistics

using System.Collections.Generic;
using System.Linq;

namespace SpaceMining
{
    public class ShipColumn
    {
        private Queue<CargoShip> ships;

        public ShipColumn() => ships = new Queue<CargoShip>();

        public void Enqueue(CargoShip ship) => ships.Enqueue(ship);

        public CargoShip PeekHead() => ships.Count > 0 ? ships.Peek() : null;

        public CargoShip TakeHead() => ships.Count > 0 ? ships.Dequeue() : null;

        public int Count => ships.Count;

        public bool IsEmpty => Count == 0;

        public IEnumerable<CargoShip> GetVisible(int max = 3) => ships.Take(max);
    }
}
