# GDD — Reward System

**Status**: Approved
**Last updated**: 2026-05-11
**Source files**: `Assets/_Project/Scripts/Core/RewardSystem.cs`, `Data/Modifier.cs`, `Data/LevelData.cs`

---

## Overview

The reward system creates a mid-level surprise moment — a mini-lottery that interrupts the mining flow and changes the game's tempo. Two ore blocks in the grid are secretly tagged as Chest and Key. When both are mined (in any order), a random modifier is drawn from either a buff or nerf pool and applied instantly to active systems. The reveal uses a pause-and-popup sequence that gives the modifier a moment to register before play resumes.

---

## Player Fantasy

The reward is the game's heartbeat reset. Just when the player has settled into a rhythm, a chest or key reveal interrupts with "wait — something's about to change." The book reveal (buff vs. nerf) adds a lottery beat: the blue book means relief; the red book means scramble. A well-timed buff feels earned; a well-timed nerf feels like the game is alive.

---

## Detailed Rules

### Setup

On `Start()` (if `LevelData.enableChestReward == true`):
- Two distinct blocks are selected at random from all non-null ore blocks in the grid
- One receives `SpecialMarker.Chest`; the other receives `SpecialMarker.Key`
- Selection is invisible to the player until the block is mined

### Discovery

`GridManager.OnBlockMined()` calls `RewardSystem.OnSpecialMined(block)` when a block's `specialMarker` is not `None`.

On chest mine:
1. `_chestFound = true`
2. `trackerUI.MarkChestFound()` — updates the HUD tracker
3. `revealUI.ShowReveal(chestSprite, "Chest Found", CheckBothFound)` — pauses game, shows popup

On key mine:
1. `_keyFound = true`
2. `trackerUI.MarkKeyFound()`
3. `revealUI.ShowReveal(keySprite, "Key Found", CheckBothFound)` — pauses game, shows popup

### Reward Trigger

`CheckBothFound()` is the callback passed to `ShowReveal`. It fires after the popup is dismissed.

```
if _bookGranted → return (already rewarded once per level)
if !_chestFound || !_keyFound → return (wait for both)

pick isBuff:
  if forceBuffOnly → true
  if forceNerfOnly → false
  else → Random.value < 0.5

pick modifier = random element from (isBuff ? buffs : nerfs)
ApplyModifier(modifier)
ShowReveal(bookSprite, modifier.label, () → trackerUI.ShowModifier(...))
```

The book reveal is a second popup that fires after `CheckBothFound` — this creates a two-beat sequence: "Chest Found" → dismiss → "Key Found" → dismiss → book reveal → modifier applied.

### Modifier Application

`ApplyModifier(Modifier m)` applies the modifier immediately to the live systems:

| ModifierType | Effect |
|---|---|
| `SlotCount` | `slotManager.ChangeSlotCount((int)m.magnitude)` |
| `DroneSpeed` | `droneManager.droneSpeed *= (1f + m.magnitude)` |
| `AttackDuration` | `droneManager.droneAttackDuration *= (1f + m.magnitude)` |
| `SpawnInterval` | `droneManager.spawnInterval *= (1f + m.magnitude)` |

The `Modifier.isBuff` field controls which sprite (blue/red book) is shown — it is a display hint only, not a formula sign. The designer is responsible for setting magnitude correctly:

| Intent | ModifierType | Magnitude sign |
|---|---|---|
| More slots (buff) | SlotCount | positive (+1) |
| Fewer slots (nerf) | SlotCount | negative (-1) |
| Faster drones (buff) | DroneSpeed | positive (+0.5 → 1.5× speed) |
| Slower drones (nerf) | DroneSpeed | negative (-0.3 → 0.7× speed) |
| Shorter attack (buff) | AttackDuration | negative (-0.3 → 0.7× duration) |
| Longer attack (nerf) | AttackDuration | positive (+0.5 → 1.5× duration) |
| Shorter interval (buff) | SpawnInterval | negative (-0.3 → 0.7× interval) |
| Longer interval (nerf) | SpawnInterval | positive (+0.5 → 1.5× interval) |

---

## Formulas

### Modifier Multiplier

For speed/duration/interval types:
```
new_value  =  old_value × (1 + magnitude)
```

For slot count:
```
new_count  =  max(1,  old_count + (int)magnitude)
```

### Buff/Nerf Selection Probability

```
P(buff)  =  0.5   (unless forceBuffOnly or forceNerfOnly override)
```

---

## Edge Cases

| Situation | Behavior |
|---|---|
| Chest and Key are the same block (impossible by construction) | `do { j = Random(…) } while (j == i)` guarantees distinct indices |
| Chest or Key block is mined before the other, then level ends | Reward callback fires on dismiss; `CheckBothFound` returns early because `!_keyFound` — no reward granted |
| Buff or nerf pool is empty | `LogWarning` logged; no modifier applied; book popup is still shown with an empty label |
| `_bookGranted` already true when `CheckBothFound` fires again | Early return — reward fires exactly once per level |
| `enableChestReward = false` | `trackerUI` hidden on `Start()`; `SelectSpecialBlocks()` never called; no blocks tagged |
| Modifier reduces `droneSpeed` to near zero | No clamp — designer must ensure nerf magnitude stays above -0.9 to avoid near-zero speed |
| Level clears while a `ShowReveal` popup is open | `ShowLevelClearedAfterDelay` uses `WaitForSecondsRealtime` and then calls `RequestPause()`; the pause stack handles the layered pauses correctly |

---

## Dependencies

- **Level Configuration** — `enableChestReward`, `forceBuffOnly`, `forceNerfOnly` from `LevelData`; buff/nerf pools configured as `List<Modifier>` on `RewardSystem` in Inspector
- **Grid System** — `GetAllBlocks()` for random selection; `OnSpecialMined()` callback on mine
- **Mining Drone System** — `droneSpeed`, `droneAttackDuration`, `spawnInterval` are modified directly
- **Cargo Slot System** — `ChangeSlotCount()` called for SlotCount modifiers
- **Game State Machine** — `RequestPause()` / `ReleasePause()` via `GameManager` (through `RewardRevealUI`)
- **UI / HUD** — `RewardRevealUI` (pause + popup) and `RewardTrackerUI` (chest/key/book tracker)

---

## Tuning Knobs

| Knob | Location | Safe range | Effect |
|---|---|---|---|
| Buff pool composition | `RewardSystem.buffs` (Inspector) | ≥ 1 entry | More variety = less predictable; pure slot-buff pool = always meaningful |
| Nerf pool composition | `RewardSystem.nerfs` (Inspector) | ≥ 1 entry | Avoid magnitudes that break the session (e.g. speed × 0.1) |
| `forceBuffOnly` on Level 1 | `LevelData` | true | Ensures tutorial reward is positive; avoids discouraging new players |
| `autoDismissSeconds` | `RewardRevealUI` | 1.5 – 3s | Longer = more time to read the modifier; shorter = less interruption |

---

## Acceptance Criteria

- [ ] Two distinct blocks in the grid receive Chest and Key markers on level load (when `enableChestReward = true`)
- [ ] Mining the chest triggers the chest popup; mining the key triggers the key popup (in any order)
- [ ] The book reveal (modifier popup) fires only after both chest and key have been found and their popups dismissed
- [ ] The reward modifier fires exactly once per level regardless of how many times `CheckBothFound` is called
- [ ] `SlotCount` modifier correctly expands or contracts the slot count mid-level
- [ ] `DroneSpeed`, `AttackDuration`, and `SpawnInterval` modifiers take effect on the next drone dispatch
- [ ] `forceBuffOnly = true` always selects from the buff pool regardless of `Random.value`
- [ ] `enableChestReward = false` leaves all blocks without special markers and hides the tracker UI
- [ ] Game remains playable (no freeze) if both popups and the level-clear state trigger in close succession
