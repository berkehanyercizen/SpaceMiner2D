# Systems Index: SpaceMiner

> **Status**: Approved
> **Created**: 2026-05-11
> **Last Updated**: 2026-05-11
> **Source Concept**: design/gdd/game-concept.md

---

## Overview

SpaceMiner is a mobile puzzle-logistics game with a tight mechanical scope — 12 active systems
covering a grid, a ship queue, cargo slots, mining drones, a reward layer, and their supporting
UI/audio/flow infrastructure. All 12 active systems are already implemented; this index exists
to formalize their design specs so the codebase can be extended, debugged, and handed off
without relying solely on reading the source. Two additional systems (Power-up, Save/Load) are
in backlog for a later development pass.

The core loop is: **Queue → Dispatch → Mine → Chain → Win/Lose**. Every system either
drives one step of that loop or makes it visible and accessible to the player.

---

## Systems Enumeration

| # | System Name | Category | Priority | Status | Design Doc | Depends On |
|---|---|---|---|---|---|---|
| 1 | Level Configuration | Core | MVP | Implemented | design/gdd/level-configuration.md | — |
| 2 | Grid System | Gameplay | MVP | Implemented | design/gdd/grid-system.md | Level Configuration |
| 3 | Ship Queue System | Gameplay | MVP | Implemented | design/gdd/ship-queue-system.md | Level Configuration |
| 4 | Cargo Slot System | Gameplay | MVP | Implemented | design/gdd/cargo-slot-system.md | Level Configuration |
| 5 | Mining Drone System | Gameplay | MVP | Implemented | design/gdd/mining-drone-system.md | Grid System, Cargo Slot System |
| 6 | Game State Machine | Core | MVP | Implemented | design/gdd/game-state-machine.md | Grid, Ship Queue, Cargo Slot, Mining Drone |
| 7 | Reward System | Gameplay | MVP | Implemented | design/gdd/reward-system.md | Grid System, Mining Drone System |
| 8 | UI / HUD | UI | MVP | Implemented | design/gdd/ui-hud.md | All gameplay systems |
| 9 | Audio System | Audio | MVP | Implemented | design/gdd/audio-system.md | — |
| 10 | Settings System (inferred) | Core | MVP | Implemented | design/gdd/settings-system.md | Audio System |
| 11 | Scene Flow (inferred) | Core | MVP | Implemented | design/gdd/scene-flow.md | Game State Machine |
| 12 | Tutorial System (inferred) | Meta | MVP | Implemented | design/gdd/tutorial-system.md | UI/HUD, Game State Machine |
| 13 | Power-up System | Gameplay | Future | Partial | — | Grid System, Cargo Slot System |
| 14 | Save / Load System | Persistence | Future | Not Started | — | Level Configuration, Game State |

---

## Categories

| Category | Description |
|---|---|
| **Core** | Foundation systems everything depends on — config, state, scene management |
| **Gameplay** | The systems that make the game fun — grid, queues, slots, drones, rewards |
| **UI** | Player-facing information displays — HUD, visualizers, menus |
| **Audio** | Sound and music systems |
| **Meta** | Systems outside the core loop — tutorial, accessibility |
| **Persistence** | Save state and continuity |

---

## Priority Tiers

| Tier | Definition |
|---|---|
| **MVP** | Required for the case study submission — everything already implemented |
| **Future** | Planned improvements for a later development pass, not in case study scope |

---

## Dependency Map

### Foundation Layer (no dependencies)

1. **Level Configuration** — all other systems read from it; must be designed first
2. **Audio System** — standalone singleton; no gameplay dependencies

### Core Layer (depends on Foundation)

3. **Grid System** — reads level config for grid dimensions, ore layout, availability
4. **Ship Queue System** — reads level config for ship queue layout and colors
5. **Cargo Slot System** — reads level config for slot count; expandable via modifiers
6. **Settings System** — controls Audio System mute state; reads/writes PlayerPrefs

### Feature Layer (depends on Core)

7. **Mining Drone System** — spawned from Cargo Slots; pathfinds to Grid ore
8. **Reward System** — triggers when Grid special blocks are mined by Drones
9. **Game State Machine** — monitors Grid, Ship Queue, Cargo Slots, and Drone completion

### Presentation Layer (depends on Features)

10. **UI / HUD** — visualizes Grid, Ship Queue, Cargo Slots, Reward state, Game State
11. **Scene Flow** — wraps Game State transitions into scene loads and menu navigation

### Polish Layer (depends on everything)

12. **Tutorial System** — overlays UI/HUD with step-by-step guidance on Level 1

---

## Recommended Design Order

| Order | System | Priority | Layer | Est. Effort |
|---|---|---|---|---|
| 1 | Level Configuration | MVP | Foundation | S |
| 2 | Grid System | MVP | Core | M |
| 3 | Ship Queue System | MVP | Core | S |
| 4 | Cargo Slot System | MVP | Core | S |
| 5 | Mining Drone System | MVP | Feature | M |
| 6 | Game State Machine | MVP | Feature | S |
| 7 | Reward System | MVP | Feature | M |
| 8 | UI / HUD | MVP | Presentation | M |
| 9 | Audio System | MVP | Foundation | S |
| 10 | Settings System | MVP | Core | S |
| 11 | Scene Flow | MVP | Presentation | S |
| 12 | Tutorial System | MVP | Polish | S |

*Effort: S = 1 session, M = 2–3 sessions. All systems are already implemented — GDD authoring
is reverse-documentation from existing code, not new design.*

---

## Circular Dependencies

None found.

---

## High-Risk Systems (for future work)

| System | Risk Type | Risk Description | Mitigation |
|---|---|---|---|
| Power-up System | Design | Partial impl exists; rules for targeting non-available blocks are undefined | Document what currently exists; design the extension separately |
| Mining Drone System | Technical | No object pooling; 24+ instantiate/destroy calls per level — mobile GC risk | Pool drones before Level 2 grid size increases |
| Grid System | Technical | Hardcoded 3-column count in 4 files — extension to other column counts is fragile | Centralize in LevelData before any new level adds non-3-column layout |

---

## Progress Tracker

| Metric | Count |
|---|---|
| Total systems identified | 14 |
| Active (in case study scope) | 12 |
| Backlog / Future | 2 |
| GDDs authored | 6 |
| GDDs reviewed | 0 |
| GDDs approved | 6 |
| MVP systems documented | 6 / 12 |

---

## Next Steps

- [ ] Drop case study PDF so agent can align documentation priorities
- [ ] Author GDDs in design order — start with `/design-system level-configuration`
- [ ] Run `/design-review` on each completed GDD
- [ ] Run `/gate-check production` when all 12 active GDDs are documented
