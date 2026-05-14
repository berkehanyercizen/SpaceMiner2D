# GDD — Cargo Slot System

**Status**: Approved
**Last updated**: 2026-05-11
**Source files**: `Assets/_Project/Scripts/Core/Data/CargoSlot.cs`, `SlotManager.cs`

---

## Overview

Cargo slots are the finite resource at the centre of SpaceMiner's tension. The player places ships from the queue into slots; each ship occupies its slot until all its drones have mined their ore. The number of slots is set per level and can be changed mid-level by reward modifiers — or by the rewarded-ad monetization offer. A full board of ships that can no longer reach ore is the lose condition.

---

## Player Fantasy

Slots are the clock. Every placement is a commitment: this ship stays here until it finishes, and that blocks a slot from being used for something more urgent. The player is always one bad placement away from a deadlock they can see coming but can't stop — which is exactly the right tension for the monetization offer.

---

## Detailed Rules

### Slot States

```
Empty  →  Full (TryPlaceShip)
Full   →  Active (SetActive(true), when a drone is dispatched)
Active →  Full (SetActive(false))
Full / Active  →  Empty (Clear(), when ship is depleted)
```

| State | Meaning |
|---|---|
| `Empty` | No ship; available for placement |
| `Full` | Ship docked; drones not yet dispatched this interval |
| `Active` | Ship docked; at least one drone currently in flight |

### Placement

`SlotManager.TryPlaceShip(slotIndex, ship)` returns true only if:
- `slotIndex` is valid (0 ≤ index < slotCount)
- The target slot is currently `Empty`

On success: ship is placed, state becomes `Full`, ship-placed SFX fires.

### Clearing

`SlotManager.ClearSlot(index)` calls `CargoSlot.Clear()`, which:
- Removes the ship reference
- Returns the `CargoShip` instance (used by `DroneManager` during cleanup)
- Resets state to `Empty`

### Dynamic Slot Count

`SlotManager.ChangeSlotCount(delta)` expands or contracts the slot array:
- New count = max(1, current + delta)
- **Existing slots are preserved** in their original index positions
- New slots (on expansion) are added at the end, starting `Empty`
- On contraction, the highest-index slots are removed. If they contain ships, those ships are lost (the scenario where contraction-mid-level is triggered by a nerf modifier).

### Deadlock Detection

`AreAllSlotsFull()` returns true when every slot is `Full` or `Active`. Combined with `GameManager.CanAnyShipMine() == false` and `!IsQueueEmpty()`, this is the lose condition.

---

## Formulas

### Slot Availability

```
empty_slots  =  slotCount - count(slots where IsEmpty == false)
```

### Monetization Breakeven

```
continue_value  =  1 additional slot × remaining_ore_of_correct_color > 0
```
The ad offer is meaningful only when at least one queued ship's color still has reachable ore. The current implementation always offers it on deadlock, leaving this evaluation to the player.

---

## Edge Cases

| Situation | Behavior |
|---|---|
| `TryPlaceShip` on a Full slot | Returns false; queue head is NOT consumed |
| `ChangeSlotCount(-1)` reduces count below current ships | Ships in removed slots are lost; this is intentional for nerf modifiers |
| `ChangeSlotCount(0)` | No-op (new count == old count check exits early) |
| `ChangeSlotCount` called with delta that would bring count below 1 | Clamped to 1 via `Mathf.Max(1, ...)` |
| Slot index out of range in `GetSlot()` | Returns null; callers must null-check |

---

## Dependencies

- **Level Configuration** — `LevelData.slotCount` sets initial slot count
- **Ship Queue System** — ships from the queue are placed into slots
- **Mining Drone System** — `DroneManager` calls `ClearSlot()` when a ship depletes; calls `SetActive()` during dispatch
- **Reward System** — applies `ModifierType.SlotCount` via `ChangeSlotCount()`
- **Game State Machine** — `AreAllSlotsFull()` feeds the lose-condition check
- **Monetization (LevelFailedController)** — calls `ChangeSlotCount(+1)` on ad confirm

---

## Tuning Knobs

| Knob | Location | Safe range | Effect |
|---|---|---|---|
| `slotCount` | `LevelData` | 2–6 | Core difficulty lever — fewer slots = harder, more slots = forgiving |
| SlotCount buff magnitude | `RewardSystem.buffs` list | +1 | Grants relief mid-level |
| SlotCount nerf magnitude | `RewardSystem.nerfs` list | -1 | Can trigger immediate near-deadlock if timed poorly |

---

## Acceptance Criteria

- [ ] Placing a ship into an empty slot sets its state to `Full` and plays the ship-placed SFX
- [ ] Placing a ship into an occupied slot returns false and does not take the ship from the queue
- [ ] When a ship depletes, `ClearSlot()` sets the slot back to `Empty`
- [ ] `AreAllSlotsFull()` returns true only when every slot has a ship
- [ ] `ChangeSlotCount(+1)` during play adds an empty slot at the end without affecting existing slots
- [ ] `ChangeSlotCount(-1)` removes the last slot; if it contained a ship, that ship is gone
- [ ] Slot count is clamped to a minimum of 1 regardless of modifier magnitude
