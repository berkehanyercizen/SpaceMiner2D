# SpaceMiner — Full Health Check Report

**Date**: 2026-05-11
**Project stage**: Production (brownfield — code-ahead-of-docs)
**Engine**: Unity 6000.4.5f1 (Unity 6 LTS), URP 2D, New Input System
**CCGS Framework**: Fully installed — 49 agents, 72 skills, 12 hooks
**Review mode**: Lean

---

## Executive Summary

SpaceMiner is a **fully playable, production-quality Unity 6 puzzle-logistics game**.
The core game loop is complete, all major systems are implemented across 31 C# scripts,
2 differentiated levels exist with ScriptableObject-driven configuration, and the game
supports both touch and mouse input. The code is well-organized, namespaced, and
clearly data-driven.

**The problem**: the game was built entirely without CCGS documentation, tests, or
formal process. The CCGS framework is installed but misconfigured — engine routing
was pointing at Godot docs (now fixed), technical preferences are blank, no GDDs
exist, no ADRs exist, and zero unit tests have been written.

**The risk**: extending any system right now means working without a spec, without
tests, and without documented reasoning for past decisions. Bugs can regress silently.
New contributors (or agents) have no written reference for how the game works.

**The path forward**: the adoption plan at `docs/adoption-plan-2026-05-11.md` gives a
numbered migration checklist. The most urgent single action is running `/setup-engine unity 6`.

---

## Game Overview

| Property | Value |
|---|---|
| Genre | Puzzle / Logistics |
| Core mechanic | Tap ships from queue into slots; drones pathfind to matching-color ore |
| Win condition | Mine all ore blocks on the grid |
| Lose condition | All slots full, no ship can mine remaining ore, queue non-empty |
| Grid size | 6×4 default (configurable via LevelData ScriptableObject) |
| Ship queues | 3 columns |
| Ore colors | 5 (Color1–Color5) |
| Reward system | Hidden chest + key markers trigger random buff/nerf modifiers |
| Modifier types | SlotCount, DroneSpeed, AttackDuration, SpawnInterval |
| Scenes | MainMenu, Level1, Level2 + tutorial overlay |
| Input | New Input System — touch + mouse |

---

## System Inventory

All systems implemented. Status reflects code quality, not design documentation.

| System | Scripts | Code Status | GDD Status |
|---|---|---|---|
| Grid | `GridManager`, `OreBlock`, `GridPathfinder`, `GridVisualizer` | ✅ Solid | ❌ Missing |
| Ship Queue | `CargoShip`, `ShipColumn`, `ShipQueueVisualizer` | ✅ Solid | ❌ Missing |
| Slot | `CargoSlot`, `SlotManager`, `SlotVisualizer` | ✅ Solid | ❌ Missing |
| Drones | `MiningDrone`, `DroneManager` | ⚠️ Issues | ❌ Missing |
| Reward | `RewardSystem`, `Modifier`, `RewardRevealUI`, `RewardTrackerUI` | ⚠️ Issues | ❌ Missing |
| Game State | `GameManager` | ✅ Solid | ❌ Missing |
| Input | `QueueInputController` | ⚠️ Issues | ❌ Missing |
| Audio | `AudioManager`, `GameAudioSettings`, `SettingsToggleButton` | ⚠️ Issues | ❌ Missing |
| UI / Menus | `MainMenuController`, `PauseMenuController`, `TutorialController`, `SpeedUpController` | ✅ Solid | ❌ Missing |
| Level Config | `LevelData` (ScriptableObject) | ✅ Solid | ❌ Missing |
| VFX / Util | `StarfieldGenerator`, `PulsingText`, `ShipQueueRef` | ⚠️ Issues | N/A |

---

## Issues — Full List With Criticality Ratings

---

### 🔴 CRITICAL — Fix before any feature expansion

These issues can cause **game freezes, crashes, or silent data corruption**. They should
be addressed before the codebase is extended further.

---

**C1 — Zero unit tests**
- **Files**: Entire codebase; Test Framework 1.6.0 installed but unused
- **Risk**: Any refactor (fixing hardcoded column count, adding a new ore color, changing
  pathfinding) has no safety net. Bugs in A*, reward modifier application, and state
  transitions can regress silently and reach players.
- **Fix**: Run `/test-setup unity`. Priority test targets: `GridPathfinder.FindPath()`,
  `RewardSystem` modifier application, `GameManager` win/lose transitions.

---

**C2 — Texture2D memory leaks**
- **Files**: `GridVisualizer.cs:54–57`, `StarfieldGenerator.cs:66–80`
- **Risk**: Both scripts create a `new Texture2D(...)` in `Start()` and store it as a
  local or field reference but never call `Destroy()` on it. On mobile, every scene
  reload accumulates these unfreed native textures. On a device with limited GPU memory
  this causes escalating RAM usage and eventual crash or OOM kill.
- **Fix**: Store the texture in a field, call `Destroy(textureField)` in `OnDestroy()`.
  Alternatively, replace with a built-in `Texture2D.whiteTexture` reference where possible.

---

**C3 — Hardcoded column count = 3 in 4+ files**
- **Files**: `GridManager.cs`, `ShipQueueVisualizer.cs`, `DroneManager.cs`, `MiningDrone.cs`
- **Risk**: `LevelData` has no column count field, and `ShipQueueVisualizer` hardcodes a loop of 3.
  If a level design ever calls for 2 or 4 columns, every hardcoded site must be hunted and changed
  manually. Missing one causes visual desync or null reference exceptions at runtime.
- **Fix**: Add `columnCount` to `LevelData`. Pass it through to all visualizers. Extract
  the magic `3` into a single constant derived from `LevelData`.

---

**C4 — `Time.timeScale` double-manipulation risk**
- **Files**: `RewardRevealUI.cs`, `GameManager.cs`
- **Risk**: `RewardRevealUI` sets `Time.timeScale = 0` when the reward popup appears and
  restores it to `1` on dismiss. `GameManager` also changes `timeScale` for win/lose sequences.
  If a reward reveal fires at the same frame as a game-over condition, the two coroutines
  can fight over `timeScale` — leaving it stuck at `0` with no recovery path. The game
  freezes in a visually-running state (real-time timers still tick) with no way out
  except force-quit.
- **Fix**: Centralize `timeScale` control in `GameManager`. Any system that needs to pause
  time posts a request; `GameManager` manages a pause stack and resolves conflicts.

---

**C5 — AudioManager singleton duplication in editor**
- **Files**: `AudioManager.cs`
- **Risk**: `AudioManager` uses `DontDestroyOnLoad` but has no instance guard at the top
  of `Awake()`. When the Unity editor exits and re-enters Play mode (common during dev),
  a new `AudioManager` is created without destroying the previous one. After 3–4 Play mode
  re-entries, all audio events play 4× simultaneously. This is a silent bug that grows worse
  the longer a dev session runs.
- **Fix**: Standard singleton guard in `Awake()`:
  ```csharp
  if (Instance != null && Instance != this) { Destroy(gameObject); return; }
  Instance = this;
  DontDestroyOnLoad(gameObject);
  ```

---

**C6 — `GridPathfinder.FindPath()` infinite-spin on blocked ore**
- **Files**: `GridPathfinder.cs`, `DroneManager.cs`
- **Risk**: When A* cannot reach a target (ore surrounded by unavailable blocks), it returns
  a single-element list containing the drone's current position. `DroneManager`'s coroutine
  interprets "arrived at waypoint 0" as success and immediately dispatches a new drone to the
  same unreachable ore. This creates an infinite loop of drone spawns for one ore block,
  consuming CPU and drone visuals until the level is manually reloaded.
- **Fix**: `FindPath()` should return `null` (or an explicit `PathResult.Unreachable` type)
  when no path exists. `DroneManager` should mark the ore block as `locked = true` on
  `null` result and not retry.

---

### 🟠 MAJOR — Address within current sprint

These issues **will cause bugs or silent misbehavior** under normal gameplay conditions
but are not guaranteed to crash every session.

---

**M1 — Hardcoded scene name strings**
- **Files**: `GameManager.cs`, `MainMenuController.cs`, `PauseMenuController.cs`
- **Risk**: `"Level1"`, `"Level2"`, `"MainMenu"` appear as string literals in multiple files.
  Renaming a scene in Unity (e.g., splitting Level2 into Level2a/Level2b) breaks navigation
  silently at runtime — no compile error, just a blank/frozen transition.
- **Fix**: Create a `static class SceneNames` with `const string` entries. All scene loads
  reference constants.

---

**M2 — Hardcoded animator state strings**
- **Files**: `MiningDrone.cs`, `ShipQueueVisualizer.cs`
- **Risk**: `"MOVE"`, `"ATTACK"`, `"IDLE"` are passed to `Animator.Play()` as string literals.
  Renaming an animation state in the Animator Controller silently breaks the drone — it
  plays no animation and produces no error until the frame where the state should trigger.
- **Fix**: Cache `Animator.StringToHash("MOVE")` etc. in `Awake()`. Store as `int` fields.
  This also gives a minor performance benefit.

---

**M3 — No object pooling for drones**
- **Files**: `DroneManager.cs`
- **Risk**: A new drone `GameObject` is `Instantiate()`d for every ore block mined and
  `Destroy()`d after. On a 6×4 grid with 24 ore blocks, that's 24 Instantiate/Destroy
  calls per level. On mobile, GC pressure from repeated heap allocations causes frame
  spikes. Grids larger than 6×4 (as level count grows) will make this progressively worse.
- **Fix**: Implement a simple `DronePool` using Unity's `ObjectPool<T>` (available in
  Unity 6). Pool size = max concurrent drones (one per active slot = 4–8).

---

**M4 — `DroneManager.SpawnDrone()` no prefab null-check**
- **Files**: `DroneManager.cs`
- **Risk**: If `dronePrefab` is accidentally unassigned in the Inspector (easy during scene
  duplication or prefab variant creation), `Instantiate(null)` throws a
  `NullReferenceException` mid-game with no descriptive message. The slot the drone
  was supposed to occupy stays in `Active` state permanently — the slot is stuck.
- **Fix**: Add a guard at the top of `SpawnDrone()`:
  ```csharp
  if (dronePrefab == null) { Debug.LogError("[DroneManager] dronePrefab not assigned"); return; }
  ```

---

**M5 — `QueueInputController` no camera null-check**
- **Files**: `QueueInputController.cs`
- **Risk**: `Camera.main` returns `null` if the main camera's tag is changed or a camera
  is accidentally deleted. Without a null guard, every input frame throws a silent
  `NullReferenceException`. All ship placement input stops working with no visible error
  to the player.
- **Fix**: Null-check `Camera.main` in `Start()` and log an explicit error. Cache the
  result rather than calling `Camera.main` every frame (it's a `FindGameObjectsWithTag`
  call internally).

---

**M6 — No async scene loading**
- **Files**: `GameManager.cs`, `MainMenuController.cs`
- **Risk**: All scene loads are synchronous (`SceneManager.LoadScene(name)`). On a mobile
  device with a larger scene or slower storage, this causes a visible freeze frame (50–200ms
  black screen) during every scene transition.
- **Fix**: Replace with `SceneManager.LoadSceneAsync()` and show a simple loading indicator
  or fade while the scene loads in the background.

---

**M7 — `LevelData.visibleQueueRows` never read**
- **Files**: `LevelData.cs`, `ShipQueueVisualizer.cs`
- **Risk**: Designers can set `visibleQueueRows` in the ScriptableObject Inspector, but
  `ShipQueueVisualizer` ignores it and always displays a hardcoded row count. Any
  level-specific queue depth tuning silently has no effect. This will confuse any designer
  who tries to use the field.
- **Fix**: Read `LevelData.visibleQueueRows` in `ShipQueueVisualizer.Start()` and use it
  to drive the visible row layout.

---

**M8 — No GDDs for any system**
- **Impact**: Cannot use `/design-review`, `/gate-check`, `/balance-check`, `/review-all-gdds`,
  or `/create-stories`. Any new agent or contributor has no written spec for how the game works.
- **Fix**: Run `/reverse-document design Assets/_Project/Scripts/Core/[script].cs` for each
  of the 6 core systems. See `docs/adoption-plan-2026-05-11.md` Step 2.3.

---

**M9 — CCGS engine routing unconfigured**
- **Files**: `.claude/docs/technical-preferences.md` (all `[TO BE CONFIGURED]`)
- **Impact**: All 49 agents that read technical-preferences.md for specialist routing get empty
  values. `/dev-story`, `/code-review`, `/architecture-decision` cannot dispatch the correct
  engine-specialist agent. They'll fall back to generic behavior.
- **Fix**: Run `/setup-engine unity 6`. This is the single most leveraged action available right now.

---

**M10 — CCGS `src/` path mismatch**
- **Impact**: CCGS skills that scan `src/` for code context find nothing. Unity source is at
  `Assets/_Project/Scripts/Core/`, not `src/`. Skills like `/reverse-document` need to be
  called with the full Unity path explicitly.
- **Fix**: Add a note to the root `CLAUDE.md` clarifying the path mapping for agents.

---

### 🟡 MODERATE — Prioritize in next sprint

These issues affect **code quality, maintainability, and developer experience** but do not
cause runtime crashes under normal gameplay.

---

**Mo1 — `PlayerPrefs` keys are hardcoded strings**
- **Files**: `GameAudioSettings.cs`
- `"music_on"`, `"sfx_on"` — typos in any file silently lose settings. Use a `const` class.

**Mo2 — No volume sliders**
- **Files**: `AudioManager.cs`, `SettingsToggleButton.cs`
- Only mute/unmute; no granular volume control. Standard expectation for mobile games.

**Mo3 — `GridVisualizer.OnValidate()` rebuilds entire grid**
- **Files**: `GridVisualizer.cs`
- Fires on every Inspector value tweak in Edit mode. On larger grids, this creates noticeable
  lag in the Unity Editor during level authoring.

**Mo4 — `RewardSystem.SelectSpecialBlocks()` continues after warning**
- **Files**: `RewardSystem.cs`
- Logs a warning if fewer than 2 ore blocks are available but continues execution anyway.
  If the grid is very small (e.g., debug 2×2), the null check downstream fails.

**Mo5 — No save/load system**
- All player progress is lost on close. No level unlock state, no high scores, no settings
  persistence beyond `PlayerPrefs` mute toggle.

**Mo6 — Visual Scripting package installed but unused**
- `Packages/manifest.json` includes `com.unity.visualscripting`. It adds ~8MB to the build
  and slows compilation. Remove if not planned for use.

**Mo7 — Empty entity registry**
- **Files**: `design/registry/entities.yaml`
- Skeleton exists. Without populated entries, `/consistency-check` cannot validate that
  cross-document references (ore colors, modifier types) are consistent.

**Mo8 — Company name is `DefaultCompany`**
- **Files**: `ProjectSettings/ProjectSettings.asset`
- Appears in the app store listing, About screen, and file paths on device storage.

**Mo9 — No ADRs for any architectural decision**
- **Files**: `docs/architecture/` (empty)
- Why singleton managers? Why direct references vs event bus? Why instantiate/destroy drones?
  None of these decisions are documented. Future contributors will repeat the analysis.

---

### 🔵 MINOR — Tech debt backlog

These are **low-urgency improvements** that improve long-term maintainability.

| # | Issue | File |
|---|---|---|
| Mi1 | Turkish comments — mixed-language codebase | `ShipQueueVisualizer.cs:19` |
| Mi2 | No XML `///` doc comments on public APIs | All manager classes |
| Mi3 | `ShaderUtilities.ID_FaceColor` used via reflection — fragile with TMP upgrades | `ShipQueueVisualizer.cs:150` |
| Mi4 | No input abstraction layer — directly coupled to New Input System | `QueueInputController.cs` |
| Mi5 | `Assets/_Project/Prefabs/` is empty — ship prefabs live under Art/ | Asset organization |
| Mi6 | No CI/CD pipeline — no automated test gate on merge | Missing |

---

## CCGS Framework Status

| Component | Status | Notes |
|---|---|---|
| Framework installed | ✅ | 49 agents, 72 skills, 12 hooks |
| Engine reference (Unity) | ✅ | `docs/engine-reference/unity/VERSION.md` — Unity 6.3 LTS |
| Engine reference pointer | ✅ Fixed | `docs/CLAUDE.md` now points to Unity (was Godot) |
| Review mode | ✅ Set | `lean` — phase gate reviews only |
| Technical preferences | ❌ Blank | Run `/setup-engine unity 6` |
| Game concept | ❌ Missing | Run `/brainstorm` or write manually |
| Systems index | ❌ Missing | Run `/map-systems` |
| GDDs | ❌ 0/6 | Run `/reverse-document` per system |
| ADRs | ❌ 0 | Run `/architecture-decision` |
| Control manifest | ❌ Missing | Run `/create-control-manifest` after ADRs |
| Unit tests | ❌ 0 | Run `/test-setup unity` |
| Sprint tracking | ❌ Missing | Run `/sprint-plan` |
| Session state | ✅ Created | `production/session-state/active.md` |

---

## Priority Action Order

The most leveraged single action is `/setup-engine unity 6` — it unblocks all agent
routing, naming conventions, and specialist dispatch in one step.

```
1. /setup-engine unity 6                          (5 min — unblocks all agent routing)
2. Fix C2: Texture2D leaks in GridVisualizer      (30 min — mobile crash risk)
3. Fix C4: timeScale centralization               (1 hr — freeze risk)
4. Fix C5: AudioManager singleton guard           (15 min — editor annoyance → prod risk)
5. Fix C6: GridPathfinder impossible-path guard   (30 min — infinite drone loop)
6. /reverse-document × 6 systems                 (1 session — unblocks all CCGS gates)
7. /test-setup unity + 3 test suites             (2 hrs — safety net for all above fixes)
8. /architecture-decision × 3 ADRs              (1.5 hrs — documents the why)
9. /create-control-manifest                      (30 min — enables /dev-story routing)
10. /sprint-plan                                  (30 min — formalizes remaining work)
```

---

*Generated by health check session — 2026-05-11*
*See also: `production/project-stage-report.md`, `docs/adoption-plan-2026-05-11.md`*
