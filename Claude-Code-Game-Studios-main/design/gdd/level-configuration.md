# GDD — Level Configuration

**Status**: Approved
**Last updated**: 2026-05-11
**Source file**: `Assets/_Project/Scripts/Core/Data/LevelData.cs`

---

## Overview

`LevelData` is a ScriptableObject that is the single source of truth for every tunable parameter in a level. Grid dimensions, ore layout, ship queue composition, slot count, and reward settings are all defined here and read at runtime by the systems that need them. Adding a new level means creating a new `LevelData` asset — no code changes required.

---

## Player Fantasy

Not directly player-facing. This system shapes the difficulty curve, pacing, and surprise moments the player experiences. A well-tuned `LevelData` feels like a fair puzzle; a poorly tuned one feels arbitrary or impossible.

---

## Detailed Rules

### Fields

| Field | Type | Default | Description |
|---|---|---|---|
| `gridWidth` | int | 6 | Number of columns in the ore grid |
| `gridHeight` | int | 4 | Number of rows in the ore grid |
| `slotCount` | int | 4 | Number of cargo slots at level start |
| `visibleQueueRows` | int | 3 | (Unused at runtime — reserved for visualizer) |
| `typeLayout` | string | — | Grid ore colors, one char per cell |
| `availabilityLayout` | string | — | Which cells are targetable at start |
| `shipQueueLayout` | string | — | Three ship columns, one per line |
| `isTutorialLevel` | bool | false | Enables tutorial overlay when true |
| `enableChestReward` | bool | true | Activates chest+key reward system |
| `forceBuffOnly` | bool | false | Reward always selects from buff pool |
| `forceNerfOnly` | bool | false | Reward always selects from nerf pool |

### typeLayout Format

A multi-line string with exactly `gridHeight` lines, each containing exactly `gridWidth` characters.

- `'1'`–`'5'` — ore block of that color (maps to `OreColor.Color1`–`Color5`)
- `'.'` — empty cell (no block)

Rows are ordered **top-to-bottom** in the string; the first line is the topmost grid row (highest Y index).

```
Example (6×4):
123456   ← row 3 (y=3), displayed at top
112233   ← row 2 (y=2)
332211   ← row 1 (y=1)
.1234.   ← row 0 (y=0), bottom row
```

### availabilityLayout Format

Same dimensions and character positions as `typeLayout`.

- `'1'` — block starts **available** (targetable immediately)
- `'0'` — block starts **locked** (must be unlocked by mining its available neighbors)
- `'.'` — empty cell; dot positions **must match** `typeLayout` exactly

### shipQueueLayout Format

Exactly 3 lines (one per column). Each line is a comma-separated list of `type:power` tokens.

- `type` — integer 1–5, maps to `OreColor.Color1`–`Color5`
- `power` — integer ≥ 1, number of drones the ship carries

```
Example:
1:3, 2:2, 1:1   ← column 0 (left): Color1 ship with 3 drones, then Color2 with 2, then Color1 with 1
2:2, 3:2         ← column 1 (center)
3:1, 1:3, 2:1   ← column 2 (right)
```

---

## Formulas

### Balance Constraint

For every ore color `c` present in the level:

```
Σ ore_blocks(c) == Σ ship_miningPower(c)
```

Validated by `GridManager.Awake()` with a `LogWarning` on mismatch. Levels that violate this constraint are either unwinnable (more ore than ship power) or trivially easy (more power than ore).

### Grid Cell Count

```
total_cells  = gridWidth × gridHeight
playable_cells = count of non-'.' chars in typeLayout
```

---

## Edge Cases

| Situation | Behavior |
|---|---|
| `typeLayout` row count ≠ `gridHeight` | `GridManager` logs an error and returns an empty grid |
| `availabilityLayout` dot positions don't match `typeLayout` | `GridManager` logs an error and returns an empty grid |
| `shipQueueLayout` has ≠ 3 lines | `GridManager` logs an error and returns empty columns |
| Invalid `type:power` token format | That ship is skipped with a `LogError`; rest of column still loads |
| `forceBuffOnly` and `forceNerfOnly` both true | `forceBuffOnly` wins (it is checked first in `RewardSystem`) |
| `enableChestReward = false` | Reward tracker UI is hidden; no special blocks are selected |

---

## Dependencies

- **Reads from**: Designer (created as ScriptableObject assets in Editor)
- **Read by**: `GridManager` (grid + queue), `SlotManager` (slot count), `RewardSystem` (reward flags)
- **No runtime writes** — `LevelData` is read-only at play time

---

## Tuning Knobs

| Knob | Safe range | Effect |
|---|---|---|
| `gridWidth` × `gridHeight` | 4×3 – 8×6 | Larger grids = longer levels, more pathfinding cost |
| `slotCount` | 2–6 | Fewer slots = tighter tension; more slots = easier |
| Proportion of `'1'` cells in `availabilityLayout` | 20–40% | More available = easier start; less = slower opening pace |
| `forceBuffOnly` on Level 1 | true | Ensures the tutorial reward is always positive — good for onboarding |

---

## Acceptance Criteria

- [ ] Changing `gridWidth`/`gridHeight` in a `LevelData` asset correctly resizes the grid at runtime
- [ ] A `typeLayout` with a row-count mismatch produces a `LogError` and an empty (but non-crashing) grid
- [ ] `slotCount` is respected; the slot system starts with exactly that many slots
- [ ] `shipQueueLayout` with 3 columns loads the correct ship types and powers into each column
- [ ] Balance mismatch (ore count ≠ ship power for any color) produces a `LogWarning` but does not prevent the level from loading
- [ ] `enableChestReward = false` hides the reward tracker UI and produces no chest/key blocks
- [ ] Creating a new `LevelData` asset and assigning it to a scene's `GridManager` and `SlotManager` fully configures the level without any code changes
