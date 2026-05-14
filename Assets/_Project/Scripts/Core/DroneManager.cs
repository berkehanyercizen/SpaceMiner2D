using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using TMPro;

namespace SpaceMining
{
    public class DroneManager : MonoBehaviour
    {
        public GridManager gridManager;
        public GridVisualizer gridVisualizer;
        public SlotManager slotManager;
        public GameManager gameManager;
        public SlotVisualizer slotVisualizer;
        public GameObject dronePrefab;
        public float droneSpeed = 3f;
        public float droneScale = 0.3f;
        public float spawnInterval = 0.5f;
        public float departureAnimDuration = 0.5f;
        public float droneAttackDuration = 0.5f;
        public string attackState = "ATTACK";
        public string moveState = "MOVE";

        public void RegisterShip(CargoShip ship, Vector3 slotPos, int slotIndex, GameObject shipGo)
        {
            StartCoroutine(ShipLoop(ship, slotPos, slotIndex, shipGo));
        }

        private IEnumerator ShipLoop(CargoShip ship, Vector3 slotPos, int slotIndex, GameObject shipGo)
        {
            if (dronePrefab == null)
            {
                Debug.LogError("[DroneManager] dronePrefab is not assigned.");
                yield break;
            }
            while (!ship.IsDepleted)
            {
                yield return new WaitForSeconds(spawnInterval);

                var targets = gridManager.GetTargetableBlocksOfType(ship.color);
                if (targets.Count == 0) continue;

                // Mining order: bottom row first, left-to-right within each row.
                var target = targets
                    .OrderBy(b => b.gridPosition.y)
                    .ThenBy(b => b.gridPosition.x)
                    .First();
                if (!target.TryLock()) continue;

                Vector3 currentPos = slotVisualizer != null
                    ? slotVisualizer.GetSlotWorldPosition(slotIndex)
                    : slotPos;

                var waypoints = GridPathfinder.FindPath(currentPos, target.gridPosition, gridManager, gridVisualizer);
                if (waypoints == null)
                {
                    target.Unlock();
                    continue;
                }

                ship.TryDispatchDrone();
                var label = shipGo != null ? shipGo.GetComponentInChildren<TMP_Text>() : null;
                if (label != null) label.text = ship.DronesRemaining.ToString();
                if (shipGo != null) shipGo.transform.position = currentPos;
                SpawnDrone(ship.color, currentPos, target, waypoints);
            }

            var anim = shipGo != null ? shipGo.GetComponent<Animator>() : null;
            anim?.Play(attackState);
            yield return new WaitForSeconds(departureAnimDuration);
            slotManager.ClearSlot(slotIndex);
            if (shipGo != null) Destroy(shipGo);
            gameManager?.EvaluateState();
        }

        private void SpawnDrone(OreColor color, Vector3 spawnPos, OreBlock target, List<Vector3> waypoints)
        {
            AudioManager.PlayDroneDispatched();
            HapticManager.PlayDroneDispatched();
            var go = Instantiate(dronePrefab, spawnPos, Quaternion.identity);
            go.transform.localScale = Vector3.one * droneScale;

            foreach (var col in go.GetComponentsInChildren<Collider2D>())
                col.enabled = false;
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;

            var hitbox = go.AddComponent<CircleCollider2D>();
            hitbox.isTrigger = true;
            hitbox.radius = 0.2f;

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = gridVisualizer.GetVisualColor(color);

            var anim = go.GetComponent<Animator>();
            anim?.Play(moveState);

            var drone = go.AddComponent<MiningDrone>();
            drone.speed = droneSpeed;
            drone.Initialize(target, waypoints, gridManager, gridVisualizer, anim, attackState, droneAttackDuration);
        }
    }
}
