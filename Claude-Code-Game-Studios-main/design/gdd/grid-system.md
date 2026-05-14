# GDD — Grid System

**Status**: Approved
**Last updated**: 2026-05-11
**Source files**: `Assets/_Project/Scripts/Core/GridManager.cs`, `Data/OreBlock.cs`, `GridPathfinder.cs`

---

## Overview

The ore grid is the game board. It is an N×M array of colored ore blocks, some available to mine immediately and others locked until their neighbors are cleared. Mining a block propagates availability to its orthogonal neighbors via BFS, creating a cascading unlock effect. The grid also hosts the A\* pathfinding that drones use to navigate to their targets. The grid state drives both the win condition (all mined) and the lose condition (deadlock).

---

## Player Fantasy

The grid is the puzzle. Reading the color layout at a glance — spotting which ships will unlock which columns, predicting the chain reaction of a well-timed mine — is the game's core skill expression. The best plays feel like a cascade: one mine opens two, two open four, and the board dissolves in a satisfying sequence.

---

## Detailed Rules

### Block States

Each `OreBlock` carries three independent flags:

| Flag | Meaning |
|---|---|
| `isAvailable` | Block is reachable from the current grid boundary (can be targeted) |
| `isLocked` | A drone has claimed this block; no second drone should target it |
| `isMined` | Block has been destroyed; permanent |

```
IsTargetable()  =  isAvailable  &&  !isLocked  &&  !isMined
```

### Availability Propagation (BFS)

When `OnBlockMined(block)` fires:
1. `block.Mine()` — sets `isMined = true`
2. For each **orthogonal** neighbor (N, S, E, W):
   - If the neighbor is not mined and not already available → set `isAvailable = true`
3. This is a single-step propagation, not a full flood-fill — only the immediate neighbors of the mined block are updated.

> Note: initial availability is set by `LevelData.availabilityLayout`. The designer controls which edge cells start open.

### Locking

`TryLock()` atomically checks `IsTargetable()` and sets `isLocked = true` if true. This prevents two drones from targeting the same block in the same frame. `Unlock()` releases the lock if pathfinding fails before a drone is dispatched.

### Special Markers

Two blocks per level (when `enableChestReward = true`) are randomly tagged at runtime:
- `SpecialMarker.Chest`
- `SpecialMarker.Key`

These blocks are otherwise normal ore blocks. Mining them triggers the reward system callback.

### Pathfinding (A\*)

`GridPathfinder.FindPath(startWorld, targetGrid, gm, gv)`:

- **Search space**: a bounding box around start and target, padded by 3 cells in each direction
- **Passable cells**: out-of-bounds cells, mined blocks, and the target cell itself
- **Blocked cells**: any non-mined, non-target ore block within bounds (drones fly around solid ore)
- **Returns**: ordered `List<Vector3>` world positions forming the path, or `null` if unreachable
- **Direction set**: 8-directional (cardinal + diagonal)

Movement costs:
| Direction | Cost |
|---|---|
| Cardinal (N/S/E/W) | 10 |
| Diagonal (NE/NW/SE/SW) | 14 |

Heuristic (octile distance):
```
h(a, b)  =  10 × max(|Δx|, |Δy|)  +  4 × min(|Δx|, |Δy|)
```

### Win and Balance Validation

`CheckOreMined()` is called by `DroneManager` after each mine. It iterates every cell in the grid and returns true only when all non-null blocks have `isMined == true`.

On level load, `GridManager.Awake()` validates:
```
Σ ore_blocks(color c)  ==  Σ ship_miningPower(color c)   for all colors c
```
Mismatch produces a `LogWarning` but does not block the level.

---

## Formulas

### Octile Distance Heuristic

```
h(a, b)  =  10 × max(|ax − bx|, |ay − by|)  +  4 × min(|ax − bx|, |ay − by|)
```

This is admissible for an 8-directional grid with costs 10 (straight) and 14 (diagonal), meaning A\* is guaranteed to find the optimal path.

### BFS Neighbor Count

```
neighbors(x, y)  =  {(x, y+1), (x, y−1), (x−1, y), (x+1, y)}
                     filtered to valid grid positions (0 ≤ x < width, 0 ≤ y < height)
```

### Total Ore Count

```
playable_ore  =  count of non-null OreBlock instances after ParseGridLayout
```

---

## Edge Cases

| Situation | Behavior |
|---|---|
| Target block surrounded by non-mined blocks | `FindPath` returns `null`; `DroneManager` unlocks the block and skips |
| Two drones target the same block simultaneously | `TryLock()` is atomic per frame; second drone gets `false`, continues to next available block |
| All blocks of a color are mined but ships of that color remain in queue | Ships dispatch drones; `GetTargetableBlocksOfType()` returns empty; ship loop idles until depleted |
| `OnBlockMined` called on an already-mined block | Early-return guard (`if block.isMined`) prevents double-processing |
| Grid layout string has mismatched dot positions | `ParseGridLayout` returns an empty grid with a `LogError` |
| Pathfinding start position is inside a block | A\* bounding box includes the start; start node is added to open list regardless of passability |

---

## Dependencies

- **Level Configuration** — grid dimensions and layout strings sourced from `LevelData`
- **Ship Queue System** — ship colors must correspond to ore colors present in the grid
- **Mining Drone System** — calls `GetTargetableBlocksOfType()`, `OnBlockMined()`, and uses `GridPathfinder.FindPath()`
- **Reward System** — subscribes to `OnSpecialMined()` when special markers exist
- **Game State Machine** — `AreAllOreMined()` drives the win condition; `GetTargetableBlocksOfType()` feeds the lose-condition check

---

## Tuning Knobs

| Knob | Location | Safe range | Effect |
|---|---|---|---|
| `gridWidth` × `gridHeight` | `LevelData` | 4×3 – 8×6 | Larger = longer session, more drone CPU cost |
| Starting availability ratio | `LevelData.availabilityLayout` | 15–40% of cells | More open = faster early game; less open = slower cascade start |
| Ore color distribution | `LevelData.typeLayout` | — | Clusters create satisfying chain mines; stripes create tactical pressure |
| A\* pad constant | `GridPathfinder` line 18 (`pad = 3`) | 2–5 | Larger pad = more search area, more CPU; smaller = risk of missing routes that curve wide |

---

## Acceptance Criteria

- [ ] Mining a block at position (x, y) sets `isAvailable = true` on all four orthogonal neighbors that were previously locked and not yet mined
- [ ] `IsTargetable()` returns false for a block that is mined, locked, or not available
- [ ] `FindPath` returns `null` when no valid path exists (target fully enclosed by non-mined blocks)
- [ ] `FindPath` returns a non-empty list of world positions when a valid path exists
- [ ] Two simultaneous `TryLock()` calls on the same block result in exactly one `true` return
- [ ] `AreAllOreMined()` returns true only after every block's `isMined` flag is set
- [ ] Balance mismatch on load produces a `LogWarning` and the level still loads correctly
