# GDD — Mining Drone System

**Status**: Approved
**Last updated**: 2026-05-11
**Source files**: `Assets/_Project/Scripts/Core/DroneManager.cs`, `MiningDrone.cs`

---

## Overview

The mining drone system bridges cargo slots and the ore grid. When a ship is registered in a slot, `DroneManager` starts a repeating dispatch loop: every `spawnInterval` seconds it finds the nearest targetable ore block of the ship's color, pathfinds to it, instantiates a drone, flies it to the block, and mines it. When all of a ship's drones have been dispatched, the ship departs and its slot is freed. Drones are one-shot objects — created on dispatch, destroyed on mine.

---

## Player Fantasy

Drones are the payoff. Placing a ship feels like loading a gun; watching drones stream out and ore crumble away is the satisfaction that rewards good placement decisions. A grid that chain-collapses under a well-ordered dispatch feels effortless and smart. Drones also make the deadlock threat visible — when drones stop spawning because no ore matches a ship's color, the player sees the blockage in real time.

---

## Detailed Rules

### Registration

`DroneManager.RegisterShip(ship, slotPos, slotIndex, shipGo)` starts a `ShipLoop` coroutine for the ship. The loop runs until the ship is depleted.

### ShipLoop (per registered ship)

Runs as a Unity coroutine:

```
if dronePrefab == null → LogError, yield break

while ship is not depleted:
    wait spawnInterval seconds
    targets = gridManager.GetTargetableBlocksOfType(ship.color)
    if targets empty → continue (wait again)
    target = targets[0]
    if !target.TryLock() → continue
    currentPos = slotVisualizer.GetSlotWorldPosition(slotIndex)
    waypoints = GridPathfinder.FindPath(currentPos, target.gridPosition, ...)
    if waypoints == null → target.Unlock(), continue
    ship.TryDispatchDrone()   ← drone count decremented here, after path confirmed
    SpawnDrone(color, currentPos, target, waypoints)

play departure animation
wait departureAnimDuration
slotManager.ClearSlot(slotIndex)
destroy ship GameObject
gameManager.EvaluateState()
```

Key ordering invariant: **path is confirmed before the drone count is decremented**. If pathfinding fails, the target is unlocked and no drone charge is spent.

### SpawnDrone

1. `Instantiate(dronePrefab, spawnPos, identity)` at slot world position
2. Set `localScale` to `droneScale`
3. Disable any pre-existing colliders from the prefab
4. Add a `CircleCollider2D` (trigger, radius 0.2) for visual feedback
5. Tint `SpriteRenderer` to the ore color via `GetVisualColor(color)`
6. Play `moveState` animation
7. Add `MiningDrone` component, set `speed`, call `Initialize()`

### MiningDrone Movement

`Update()` moves the drone along its waypoint list using `Vector3.MoveTowards` at `speed` world units per second. Arrival threshold: 0.05 world units from the current waypoint.

### Arrival Sequence

When the drone reaches its final waypoint:
1. Play `attackState` animation
2. `WaitForSeconds(attackDuration)` — scaled time (pauses with `timeScale = 0`)
3. `gridManager.OnBlockMined(target)` — mines the block, propagates availability
4. `gridVisualizer.VanishBlock(target)` — shrink animation + ore break VFX
5. `Destroy(gameObject)` — drone is removed

---

## Formulas

### Dispatch Rate

```
max_dispatch_rate  =  1 / spawnInterval   (drones per second, per ship)
```

At default `spawnInterval = 0.5s`, a ship dispatches at most 2 drones/second. Actual rate is lower if pathfinding takes longer than one interval (the loop waits the full interval before each attempt regardless).

### Time to Clear a Ship (ideal)

```
clear_time  ≈  miningPower × (spawnInterval + drone_travel_time + attackDuration)
```

`drone_travel_time` depends on grid distance and `droneSpeed`. At speed 3 and average distance 2 world units:

```
average_travel_time  ≈  2 / 3  ≈  0.67s
clear_time (power=3) ≈  3 × (0.5 + 0.67 + 0.5)  ≈  5s
```

### Arrival Threshold

```
arrived  =  Vector3.Distance(position, waypoint) < 0.05
```

---

## Edge Cases

| Situation | Behavior |
|---|---|
| `dronePrefab` not assigned in Inspector | `ShipLoop` logs an error and exits immediately; slot stays occupied |
| No targetable blocks of ship's color | Loop waits `spawnInterval` and retries; ship never depletes naturally if ore of its color is gone |
| `TryLock()` returns false (another drone already claimed the block) | Loop skips to next iteration; no drone dispatched, no count spent |
| `FindPath` returns null (ore enclosed) | Target is unlocked, loop skips; if ALL ore of that color is enclosed, ship idles permanently |
| Drone's target block gets mined by another drone before arrival | `OnBlockMined` guard (`if block.isMined`) prevents double mine; `VanishBlock` also guards for missing GO |
| `timeScale = 0` during drone flight | `Update` stops; `WaitForSeconds` in Arrive also stops — drones freeze mid-flight |
| Ship has miningPower = 1 | Single drone dispatched, immediate departure on completion |

---

## Dependencies

- **Grid System** — `GetTargetableBlocksOfType()`, `TryLock()`/`Unlock()`, `FindPath()`, `OnBlockMined()`
- **Cargo Slot System** — `ClearSlot()` called on ship depletion; `GetSlotWorldPosition()` for spawn position
- **Ship Queue System** — `CargoShip` is the data object the loop runs against
- **Game State Machine** — `EvaluateState()` called after each ship departs
- **Grid Visualizer** — `VanishBlock()` called after each mine; `GetVisualColor()` used for drone tint

---

## Tuning Knobs

| Knob | Field | Default | Safe range | Effect |
|---|---|---|---|---|
| Drone flight speed | `DroneManager.droneSpeed` | 3f | 1.5 – 6 | Higher = faster mines, more satisfying; lower = slower pace, more strategic reads |
| Dispatch interval | `DroneManager.spawnInterval` | 0.5s | 0.3 – 2.0s | Shorter = more frantic; longer = each dispatch feels weighty |
| Attack duration | `DroneManager.droneAttackDuration` | 0.5s | 0.2 – 1.0s | Longer = more animation payoff; shorter = snappier feel |
| Departure animation | `DroneManager.departureAnimDuration` | 0.5s | 0.2 – 1.0s | Visual beat between ship depletion and slot freeing |
| Drone visual scale | `DroneManager.droneScale` | 0.3f | 0.15 – 0.5 | Legibility on mobile vs. clutter |

All five knobs are also affected by `ModifierType` modifiers from the reward system (DroneSpeed, SpawnInterval, AttackDuration).

---

## Acceptance Criteria

- [ ] Registering a ship starts a dispatch loop that fires every `spawnInterval` seconds
- [ ] A drone is only dispatched after a valid path is confirmed — drone count is never decremented for an unreachable target
- [ ] A locked block is not targeted by a second drone in the same frame
- [ ] When a ship's `DronesRemaining` reaches 0, its slot is cleared and `EvaluateState()` is called
- [ ] `dronePrefab = null` produces a `LogError` and does not crash or corrupt slot state
- [ ] Drone arrives at target, plays attack animation, mines block, then destroys itself
- [ ] Ore break VFX fires at the block's world position on mine (via `VanishBlock`)
- [ ] Drone tint matches the ore color it is targeting
