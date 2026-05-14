# GDD — Ship Queue System

**Status**: Approved
**Last updated**: 2026-05-11
**Source files**: `Assets/_Project/Scripts/Core/Data/ShipColumn.cs`, `CargoShip.cs`, `GridManager.cs` (ParseShipQueue)

---

## Overview

The ship queue presents the player with three parallel columns of incoming cargo ships. Only the frontmost ship in each column is visible as a tap target; the player selects one and places it into an empty cargo slot. Column order and ship composition are defined per level in `LevelData.shipQueueLayout`. Once a ship leaves a column it is gone permanently — the queue is not refilled.

---

## Player Fantasy

The queue is the player's hand. Reading it ahead — spotting which color is coming next, judging when to hold a column rather than drain it — is where the game's strategic depth lives. A well-managed queue feels like reading a system; a misread queue creates the satisfying dread of watching a deadlock approach.

---

## Detailed Rules

### Structure

- Exactly **3 columns** (`ShipColumn` instances), hardcoded.
- Each column is a **FIFO queue** — ships are dispatched in the order they were defined in `LevelData`.
- Only the **head** of each column can be placed (`PeekHead()` for display; `TakeHead()` for removal).
- `GetVisible(max)` returns up to `max` ships for the visualizer; default max is 3.

### CargoShip

Each ship has two properties set at construction and never changed:

| Property | Type | Description |
|---|---|---|
| `color` | OreColor | The ore color this ship's drones target |
| `miningPower` | int | Number of drones the ship carries |

At runtime:

| Property | Description |
|---|---|
| `DronesRemaining` | Starts equal to `miningPower`; decremented by `TryDispatchDrone()` |
| `IsDepleted` | True when `DronesRemaining == 0`; ship leaves its slot when depleted |

### Placement

1. Player taps the head ship of a column — `QueueInputController` identifies the tapped column.
2. Player taps an empty cargo slot — `SlotManager.TryPlaceShip()` is called.
3. On success: `ShipColumn.TakeHead()` removes the ship from the queue; `DroneManager.RegisterShip()` starts the drone dispatch loop.
4. The next ship in the column becomes the new head.

### Depletion

When a ship's `DronesRemaining` reaches 0, `DroneManager.ShipLoop` plays the departure animation, then calls `SlotManager.ClearSlot()` — freeing the slot for the next placement.

### End of Queue

When a column is empty (`IsEmpty == true`), the player cannot place from that column. The game ends (win or lose) based on grid and slot state — not queue exhaustion.

---

## Formulas

### Balance Constraint (inherited from Level Configuration)

```
Σ miningPower for all ships of color c  ==  count of ore blocks of color c
```

### Drone Dispatch Rate (per ship)

```
max_drones_per_minute  =  60 / spawnInterval  (DroneManager.spawnInterval, default 0.5s)
actual_rate  ≤  max_drones_per_minute  (limited by ore availability and pathfinding)
```

### Time to Deplete a Ship (ideal, no waits)

```
depletion_time  =  miningPower × spawnInterval  (seconds, ignoring drone travel time)
```

---

## Edge Cases

| Situation | Behavior |
|---|---|
| All 3 columns empty, ore not yet fully mined | Game continues until win/lose condition resolves |
| Player taps a column head with no empty slot | Placement is rejected silently (no slot available) |
| Player taps an occupied slot | `TryPlaceShip` returns false; no ship taken from queue |
| `shipQueueLayout` has a column with 0 ships | That column is permanently empty from the start |
| Two columns have the same color at head | Player can choose either; both are valid plays |

---

## Dependencies

- **Level Configuration** — `shipQueueLayout` string drives queue construction
- **Cargo Slot System** — queue head is placed into a slot via `SlotManager.TryPlaceShip()`
- **Mining Drone System** — placement triggers `DroneManager.RegisterShip()`
- **Game State Machine** — `IsQueueEmpty()` checks all column counts for lose-condition evaluation

---

## Tuning Knobs

| Knob | Location | Safe range | Effect |
|---|---|---|---|
| Column length (ships per column) | `LevelData.shipQueueLayout` | 3–8 per column | Longer = more planning horizon; shorter = faster, more urgent |
| Color distribution across columns | `LevelData.shipQueueLayout` | — | Mixed columns create tactical pressure; color-grouped columns are more forgiving |
| `miningPower` values | `LevelData.shipQueueLayout` | 1–5 | Higher power ships stay in slots longer, blocking more of the slot economy |

---

## Acceptance Criteria

- [ ] Three columns load with the correct ships in the correct order as defined in `LevelData`
- [ ] Only the head ship of a column can be placed; ships behind the head are not tap-targets
- [ ] `TakeHead()` is called exactly once per successful placement — no ship is taken without a confirmed slot fill
- [ ] When a column empties, attempting to place from it does nothing and no error is thrown
- [ ] `GetVisible(3)` returns at most 3 ships per column for the visualizer, even if the column has more
- [ ] Depleted ships leave their slot, freeing it for the next placement
