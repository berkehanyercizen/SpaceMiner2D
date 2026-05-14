# Game Concept — SpaceMiner

**Status**: Approved
**Last updated**: 2026-05-11
**Engine**: Unity 6 (6000.4.5f1) — URP 2D, Mobile

---

## Overview

SpaceMiner is a mobile puzzle-logistics game where the player manages a fleet of cargo ships
to mine all the ore off a planetary surface before their shipping lanes get blocked. Ships queue
in three columns at the bottom of the screen. The player taps to dispatch them into cargo slots,
where they automatically send mining drones to the grid. The tension comes from slot management:
fill the wrong slots with the wrong ships and the grid deadlocks — you lose.

The core feeling is *satisfying logistics under pressure*: watching drones stream out, ore
blocks chain-collapse, and columns clear in the right order. A simple rule set that reveals
surprising depth the moment the grid tightens.

---

## Player Fantasy

The player feels like a space operations commander — calling ships in the right order, reading
the ore grid for patterns, and orchestrating a cascade of mining drones across the surface.
When it works, it feels effortless and smart. When slots fill up wrong, the impending deadlock
creates genuine tension. The reward reveal (chest/key) adds a mini-lottery moment that resets
the rhythm and opens a second layer of decision-making.

---

## Core Loop

```
Queue → Dispatch → Mine → Chain → Win / Lose
```

1. **Queue**: Three columns of ships scroll into view. Each ship has a color and mining power.
2. **Dispatch**: Player taps a ship from the queue head, then taps an empty slot to place it.
3. **Mine**: The slot ship automatically dispatches drones every `spawnInterval` seconds.
   Each drone pathfinds (A* 8-directional) to the nearest available ore block of matching color.
4. **Chain**: When an ore block is mined, its neighbors may become newly available, unlocking
   deeper ore and enabling other ships to mine.
5. **Win**: All ore blocks mined before the lose condition triggers.
6. **Lose**: All slots are full AND no ship currently in a slot can reach any ore block
   AND the queue still has ships waiting.

---

## Game Pillars

1. **Readable**: The grid state must be legible at a glance on a small mobile screen.
   Availability, ore color, and slot status must be immediately obvious.
2. **Rewarding flow**: Mining should feel satisfying — drones moving, ore crumbling,
   columns opening up. The game rewards good planning with visible cascades.
3. **Meaningful tension**: The lose condition (slot deadlock) must feel like the player's
   mistake, not bad luck. Grid layouts should teach the player to read color adjacency.
4. **Surprise moments**: The reward system (chest + key) adds a layer of discovery —
   finding both unlocks a random buff or nerf that changes the tempo mid-level.

---

## Mechanics Summary

### Grid System
- NxM grid of colored ore blocks (default 6×4)
- Five ore colors: Color1–Color5
- Blocks start as either "available" (reachable from outside) or locked
- When a block is mined, its neighbors recalculate availability via BFS flood-fill

### Ship Queue System
- Three parallel columns of ships
- Each column is a FIFO queue — only the front ship can be placed
- Ships have a color (matches ore) and mining power (how many drones it carries)
- Queue layout is defined per level in LevelData ScriptableObject

### Cargo Slot System
- 4 slots by default (configurable per level, expandable via modifier)
- States: Empty / Full / Active
- A ship in a slot automatically starts its drone dispatch loop

### Mining Drone System
- Each drone is spawned from the ship's slot position
- Pathfinding: A* 8-directional (diagonal movement with diagonal cost penalty)
- Target: nearest available ore block matching ship color
- On arrival: mines the ore (plays ATTACK animation), then drone is destroyed
- Ship tracks `dronesRemaining`; when 0, ship leaves the slot

### Reward System
- Each level has one hidden Chest ore block and one hidden Key ore block
- When both are mined (in any order), the reward triggers
- A popup reveals the book/chest/key and applies a random modifier:
  - **Buffs**: More slots, faster drones, shorter attack duration, shorter spawn interval
  - **Nerfs**: Fewer slots, slower drones, longer attack duration, longer spawn interval
- Modifier pool (buff vs nerf, specific modifiers) is configurable per level

### Game State Machine
- Playing → Win (all ore mined)
- Playing → Lose (deadlock: all slots full, no ship can mine, queue non-empty)
- Win/Lose → triggers scene-level UI response

---

## Platform & Scope

| Property | Value |
|---|---|
| Platform | Mobile (Android / iOS) primary, PC secondary |
| Input | Touch (primary), Keyboard/Mouse (PC fallback) |
| Screen | 9:16 portrait; 16:9 landscape supported |
| Levels | 2 playable levels (Level1 tutorial, Level2 challenge) |
| Session length | 2–5 minutes per level |
| Art style | 2D flat/geometric — colored blocks, sci-fi ship sprites, starfield background |
| Audio | 30 sci-fi music tracks (licensed); SFX via Unity audio events |

---

## Current Implementation State (2026-05-11)

**Built and working:**
- Complete grid system with A* pathfinding and BFS availability
- Ship queue with 3 columns, tap-to-place input
- Cargo slot system (dynamic slot count via LevelData + modifier)
- Drone system (spawn loop, pathfinding, mining animation)
- Reward system (chest/key, modifier application, reveal UI)
- Win/lose state machine
- Level 1 (tutorial) and Level 2 (challenge) playable
- Main menu, pause menu, settings (music/SFX toggle)
- ScriptableObject-driven level configuration
- Starfield background, pulsing text, drone animations

**Planned but not yet implemented:**
- Power-up system ("Asteroid Drill" — mine any non-available block)
- Monetization hooks (rewarded ad placements at deadlock moment)
- Object pooling for drones (currently instantiate/destroy per mine)
- Save/load system (player progress, level unlock state)
- Volume sliders (currently binary mute/unmute)
- Async scene loading (currently synchronous)
- SFX library (sound effects for ore mine, ship dock, drone dispatch)
- Particle effects (ore break, level clear confetti)

---

## Design Constraints

- All ore counts must equal total ship mining power per level (GridManager validates this)
- Slot count must be ≤ queue column count to prevent trivial deadlock
- Level layouts must always have at least one path to victory (playtest required)
- Reward modifier pool must balance: at least one buff and one nerf in the pool
- No hover-only UI states — all interactions must be tap-accessible

---

## Related Documents

- `design/gdd/systems-index.md` — full system breakdown (to be created via /map-systems)
- `space-mining-game-plan.md` — original 5-day development plan (historical reference)
- `production/session-state/health-check-2026-05-11.md` — current known issues
- `docs/adoption-plan-2026-05-11.md` — CCGS migration checklist
