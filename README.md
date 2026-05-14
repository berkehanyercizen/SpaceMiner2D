# SpaceMiner

A mobile-first puzzle-logistics game built in Unity 6 where you command a fleet of cargo ships and mining drones to strip a planetary surface bare — before your docking lanes deadlock and the run ends.

SpaceMiner is a hybrid of two classic mobile genres: **unblocking** (clearing a grid of obstructed targets) and **sorting** (routing the right colored unit into the right finite slot). The tension peaks at the **Full Buffer** moment, when every cargo slot is taken and only one queued ship can still reach a valid ore block — making each placement decision feel earned rather than arbitrary.

---

## How to Play

1. The planet grid is filled with colored ore blocks. Only blocks on the outer surface are initially **available** to mine.
2. Ships of varying colors arrive in a queue at the bottom of the screen.
3. Tap a ship from the queue, then tap one of the limited **cargo slots** to dock it.
4. A docked ship automatically dispatches **mining drones** that fly to ore matching its color, mine the block, and return.
5. As outer blocks are mined, inner blocks become available — opening new paths.
6. **Win:** clear the entire ore grid. **Lose:** all slots are occupied and no docked ship can still reach a matching ore block (a deadlock).

---

## Core Features

### Gameplay Systems

- **Dynamic Grid System** — Each level defines its own grid dimensions, ore color layout, and per-block availability using text-based layouts inside a `LevelData` ScriptableObject. Inner blocks unlock as outer blocks are mined via a flood-fill availability pass.
- **Cargo Ship Queue** — A scrolling queue feeds new ships from the bottom of the screen. The player sees the next several rows ahead, enabling two-step planning.
- **Cargo Slot Management** — A small, level-defined set of slots represents your docking bay. Ships can be parked here while drones work, but slots free only after every drone of that ship returns home.
- **Mining Drones** — Each docked ship launches autonomous drones that pathfind to matching ore using A\* with 8-directional movement and a diagonal cost penalty (`GridPathfinder`).
- **Deadlock Detection** — Every placement triggers a reachability check across all docked ships and all available ore blocks. When no valid path exists for any ship, the level fails.
- **Win / Lose Resolution** — A clean level-cleared screen on full grid clear, and a fail screen on slot deadlock or running out of queued ships.

### Reward & Modifier System

- **Chest + Key Rewards** — Special ore blocks tagged as `Chest` or `Key` can be sprinkled into the grid. Mining both unlocks a mid-level random modifier reveal.
- **Modifier Types** — Buffs and nerfs can change `SlotCount`, `DroneSpeed`, `AttackDuration`, or `SpawnInterval`, layering risk-reward depth on top of the base loop.
- **Per-Level Tuning** — Each `LevelData` asset can disable rewards entirely, force buffs-only, force nerfs-only, or constrain reward spawn locations to specific rows.

### Power-Up Hooks (designed, partially implemented)

- **Asteroid Drill** — Mines any one currently-locked ore block, bypassing the availability requirement. Designed to resolve "one stubborn cluster" deadlock scenarios.
- **Color Shift** — Reassigns a docked ship's color to match the most-available ore. A natural rewarded-ad / hard-currency placement at the moment of mismatch.

### Levels

Three hand-tuned levels ship with the project, all data-defined and live-tweakable in the editor:

| Level | Role | Grid | Notes |
|---|---|---|---|
| Level 1 | Tutorial | Small, 2–3 colors | Step-by-step on-screen guidance via `TutorialController`. Generous slot count. |
| Level 2 | Challenge | Larger grid, 4–5 colors | Tighter slot ratio, intentional color-cluster traps, monetization bottleneck. |
| Level 3 | Endgame | Largest grid | Designed around chest/key sequencing and forced modifier choices. |

A `LevelCatalog` ScriptableObject indexes all levels for the **Level Selection** menu, including unlock progression and per-level metadata.

### Lives & Progression

- **LivesManager** — A persistent life counter gates replays of failed levels, with a regeneration timer pattern ready for soft-currency or rewarded-ad refills.
- **LivesHUD** — Live UI indicator on the main menu and level select.
- **Persistent Save** — Level unlock state and lives survive between sessions via PlayerPrefs-backed storage.

### Monetization Hook

The game's natural bottleneck — the **Full Buffer** deadlock — is also its monetization moment. On Level 2 and beyond, the Level Failed screen surfaces a placeholder **"Continue? (+1 Docking Slot)"** rewarded-ad offer. Accepting applies a `SlotCount` modifier (the same system rewards already use) and resumes the session. The integration point is fully wired; only an ad SDK is needed for production.

### Audio

- **AudioManager** — Centralized SFX and music playback with channel volumes and mute toggles via `GameAudioSettings`.
- **30 Sci-Fi Music Tracks** — Licensed asset pack streamed from `StreamingAssets/Music/` with random shuffle per level.
- **SFX Coverage** — Ore breakage, drone dispatch and return, slot dock, ship select, reward reveal, level clear, and game over.

### Haptics

`HapticManager` provides cross-platform haptic feedback (Android / iOS) on key moments: ship dock, ore mine, deadlock failure, reward reveal. Falls back silently on platforms without rumble support.

### UI & UX

- **Main Menu** with starfield-animated background (`StarfieldGenerator`).
- **Level Selection** with locked / unlocked / completed states.
- **In-Game HUD** including pause, settings, and speed-up control.
- **Pause Menu** with resume, restart, and quit-to-menu options, all routed through `GameManager`'s pause stack so multiple overlays compose correctly.
- **Speed-Up Toggle** — Player can fast-forward drone animations via `SpeedUpController` to skip tedium without skipping decisions.
- **Pulsing Text** — Subtle attention animator used on call-to-action buttons.
- **Tutorial Overlay** — A scripted, step-gated tutorial in Level 1 that highlights queue, slot, and grid actions in sequence.
- **Reward Reveal UI** — Dedicated modal for chest+key payouts, separate from in-line modifier tracker.

### Architecture Highlights

- **Singleton-free gameplay logic** — Systems are wired through serialized references and lookups, not global statics.
- **Centralized pause control** — Only `GameManager` mutates `Time.timeScale`, via a pause stack so overlays (pause menu, level cleared, reward reveal) cannot fight each other.
- **Overlay-aware input** — Input scripts gate on `GameManager.IsOverlayActive` rather than raycast blockers, keeping the UI tree flat.
- **Data-driven levels** — All level content (grid, queue, slot count, reward rules, palette, bevel shading) lives in `LevelData` assets editable in the Inspector.
- **Per-level visuals** — Color palette and 2D shading parameters (bevel width / strength) are configurable per level for visual differentiation.

---

## Technical Notes

| Property | Value |
|---|---|
| Engine | Unity 6 (6000.4.5f1) |
| Render Pipeline | URP 2D |
| Target Platforms | Android, iOS, WebGL |
| Input | Touch (primary), Mouse (fallback) |
| Aspect Ratio | 9:16 / 9:19.5 mobile portrait |
| Pathfinding | A\* 8-directional with diagonal cost penalty |
| Data Layer | ScriptableObject-driven `LevelData` + `LevelCatalog` |
| Audio | 30 streamed music tracks + per-event SFX |
| Persistence | PlayerPrefs for lives, level unlocks, settings |

### Project Structure

```
Assets/_Project/Scripts/Core/   — gameplay systems (managers, controllers, UI)
Assets/_Project/Scripts/Core/Data/ — ScriptableObjects (LevelData, Modifier, OreColor)
Assets/Scenes/                  — MainMenu.unity, GameLevel.unity
Assets/StreamingAssets/Music/   — 30 licensed sci-fi tracks
```

### Building Locally

1. Open the project in Unity 6 (6000.4.5f1 or later).
2. `File → Build Settings → Platform` and select WebGL or Android.
3. Build & Run. For WebGL, serve the output folder via any modern browser (Chrome / Firefox); tap once if audio is silenced by autoplay policy.

---

## Designer's Note

SpaceMiner interprets the unblocking genre through a logistics lens: the ore grid is the blocked system, and the player unblocks it by dispatching mining drones in the correct color sequence. The sorting layer lives in the ship queue — color-matched ships must be placed into finite cargo slots before the lane deadlocks. The tension peaks when all slots are full and only one ship can still reach ore, creating a natural near-miss moment that feels earned rather than arbitrary. This hybrid felt like the most direct translation of "space station management" into a tactile mobile puzzle.
