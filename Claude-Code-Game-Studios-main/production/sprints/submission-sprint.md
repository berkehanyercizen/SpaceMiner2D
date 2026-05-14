# Submission Sprint — Noktune Case Study

> **Deadline**: 2026-05-13 (2 days remaining as of 2026-05-11)
> **Goal**: Submission-ready build + repo + README
> **Evaluator**: Noktune — judging game feel, clarity, product awareness
> **Build targets**: WebGL (primary) + APK (if time allows)

---

## What Submission Requires

- [ ] Playable WebGL link OR APK
- [ ] GitHub/GitLab repo (clean, readable code)
- [ ] README with Designer's Note (1 paragraph on mechanic choice)
- [ ] Level 2 monetization bottleneck — visible in-game or in design notes

---

## Priority Order

> Rule: A broken demo fails the evaluation before the evaluator reads the README.
> Fix crashes → Add juice → Complete submission materials → Build.

```
P0  Crash/freeze bugs     → game must survive a full playthrough without breaking
P1  SFX                   → single highest-impact detail ("satisfying docking sounds" = their words)
P2  Monetization hook     → explicitly required by Section 7 of the brief
P3  VFX                   → ore break + level clear = the "juicy" moments
P4  README + Designer's Note → required, ~30 min
P5  Build                 → WebGL first, APK if time
P6  GDDs (parallel)       → internal documentation, does not affect submission score
```

---

## Day 1 — 2026-05-11 (Today)

### P0 — Bug Fixes (~1.5 hrs)
Critical: these can make the game unplayable during review.

- [ ] **C4: TimeScale freeze** — centralize `Time.timeScale` control in `GameManager`; remove direct sets from `RewardRevealUI`
  - File: `Assets/_Project/Scripts/Core/RewardRevealUI.cs`, `GameManager.cs`
  - Risk: Reward popup near game-over freezes game with no recovery
  - Est: 30 min

- [ ] **C6: Pathfinder infinite spin** — `GridPathfinder.FindPath()` returns `null` on unreachable target; `DroneManager` marks ore as `locked` and stops retrying
  - File: `Assets/_Project/Scripts/Core/GridPathfinder.cs`, `DroneManager.cs`
  - Risk: Drone spams infinite spawns on a blocked ore block
  - Est: 30 min

- [ ] **C5: AudioManager duplicate** — add instance guard in `Awake()` before DontDestroyOnLoad
  - File: `Assets/_Project/Scripts/Core/AudioManager.cs`
  - Risk: Multiple AudioManagers after editor play-mode re-entry → doubled audio
  - Est: 15 min

- [ ] **M4: DroneManager prefab null-check** — guard `dronePrefab == null` before Instantiate
  - File: `Assets/_Project/Scripts/Core/DroneManager.cs`
  - Est: 10 min

---

### P1 — SFX (~2 hrs)
They explicitly mention "satisfying docking sound effects" as an evaluation criterion.
The music pack is already wired — need to add SFX AudioSource events.

**SFX needed** (find on freesound.org / kenney.nl / mixkit.co — all free):
- [ ] Ship placed into slot ("dock" thud/click)
- [ ] Drone dispatched ("whoosh" or soft launch)
- [ ] Ore block mined ("crunch" or "pop")
- [ ] Level clear (short fanfare / chime)
- [ ] Game over (low tone / alarm)
- [ ] Reward reveal (discovery chime)

**Implementation approach**: Add `AudioClip` fields to `AudioManager` + `GameAudioSettings`.
Wire each event at the call site (SlotManager, DroneManager, GameManager, RewardSystem).

- [ ] Source and import 5-6 SFX clips
- [ ] Add SFX playback method to `AudioManager`
- [ ] Wire: ship placed → `SlotManager.TryPlaceShip()`
- [ ] Wire: drone dispatched → `DroneManager.SpawnDrone()`
- [ ] Wire: ore mined → `GridManager.OnBlockMined()`
- [ ] Wire: level clear → `GameManager` win state
- [ ] Wire: game over → `GameManager` lose state
- [ ] Wire: reward reveal → `RewardRevealUI`
- [ ] Respect existing mute toggle (`GameAudioSettings.SfxEnabled`)

---

### P2 — Monetization Placeholder (~45 min)
Section 7 of the brief: "In Level 2, briefly mention WHERE a player might encounter a bottleneck."
SpaceMiner's bottleneck IS the "Full Buffer" — all slots full, no ship can mine.

- [ ] When `AreAllSlotsFull() == true && !CanAnyShipMine()` (the near-deadlock warning state, BEFORE full game-over):
  - Show a small "HELP?" banner UI at the top of screen: `"Slots full? Watch an ad for +1 slot!"`
  - Tapping it shows a placeholder popup: `"[Rewarded Ad Placeholder] +1 Docking Slot"`
  - This hooks directly into the existing `Modifier` system (SlotCount +1)
- [ ] Level 2 `LevelData`: confirm reward pool includes a buff modifier (it does — `SlotCount +1` exists)
- [ ] Add a comment in `GameManager.cs` marking the monetization hook location

---

## Day 2 — 2026-05-12

### P3 — VFX (~1.5 hrs)
"Game feel (juice)" is explicitly listed over visual fidelity.

- [ ] **Ore break effect**: When `OnBlockMined()` fires — spawn a small burst of colored particles matching the ore color. Use Unity's built-in Particle System. 3-5 particles, 0.3s lifetime.
- [ ] **Level clear celebration**: Spawn a confetti/star burst at screen center. Can reuse particle system with a different color gradient.
- [ ] **Near-deadlock warning pulse**: When slots are full and only 1 ship can still mine — pulse the active slot red/orange (already partially implemented via `SlotVisualizer`)

---

### P4 — README + Designer's Note (~30 min)
Required for submission. Keep it tight.

- [ ] Write `README.md` in the root of the SpaceMiner Unity project covering:
  - How to play (3 sentences)
  - Designer's Note (1 paragraph — why THIS mechanic for an unblocking+sorting hybrid)
  - Monetization section (where the bottleneck is, how the slot modifier hooks in)
  - Build instructions (WebGL link + how to run locally)
  - Technical notes (Unity 6, URP 2D, modular `LevelData` architecture)

**Draft Designer's Note** (to refine):
> "SpaceMiner interprets the unblocking genre through a logistics lens: the ore grid is the blocked system, and the player unblocks it by dispatching mining drones in the correct color sequence. The sorting layer lives in the ship queue — color-matched ships must be placed into finite cargo slots before the lane deadlocks. The tension peaks when all slots are full and only one ship can still reach ore, creating a natural near-miss moment that feels earned rather than arbitrary. This hybrid felt like the most direct translation of 'space station management' into a tactile mobile puzzle."

---

### P5 — Build (~1.5 hrs)

- [ ] **WebGL build** — File → Build Settings → WebGL → Build. Test in browser.
  - Verify touch emulation works in browser DevTools mobile mode
  - Check audio autoplay policy (browsers block audio until first interaction — add a "tap to start" gate if needed)
- [ ] **APK** (if time allows) — Switch to Android, configure minimum API level (Android 7 / API 24), build APK. Test on device or emulator.
- [ ] Upload WebGL to itch.io or GitHub Pages for a shareable link

---

### P6 — GDDs (parallel, as time allows)

Running in parallel with the above. Does not affect submission score but demonstrates process.
Author in design order from `systems-index.md`:

- [ ] `design/gdd/level-configuration.md` — smallest, fastest (S effort)
- [ ] `design/gdd/grid-system.md` — core system, most important (M effort)
- [ ] `design/gdd/ship-queue-system.md` — S effort
- [ ] `design/gdd/cargo-slot-system.md` — S effort
- [ ] `design/gdd/mining-drone-system.md` — M effort
- [ ] `design/gdd/reward-system.md` — M effort (documents monetization hook formally)
- [ ] Remaining systems (game state, UI, audio, settings, scene flow, tutorial) — if time

---

## Progress Tracker

| Item | Status | Notes |
|---|---|---|
| C4 TimeScale fix | ✅ Done | Pause stack in GameManager; RewardRevealUI routes through it |
| C6 Pathfinder fix | ✅ Done | FindPath returns null; Unlock() added; DroneManager checks before dispatch |
| C5 AudioManager fix | ✅ Done | Instance guard was already correct |
| M4 Prefab null-check | ✅ Done | yield break at top of ShipLoop if dronePrefab null |
| SFX — sourced clips | ⬜ Pending | Assign 6 AudioClips in Inspector (see below) |
| SFX — wired in code | ✅ Done | AudioManager has 6 static play methods; all call sites wired |
| Monetization placeholder UI | ✅ Done | LevelFailedController.cs written; panel lives on failed screen |
| VFX — ore break | ✅ Done | GridVisualizer.VanishBlock spawns oreBreakPrefab; auto color-matched to ore |
| VFX — level clear | ✅ Done | GameManager.ShowLevelClearedAfterDelay spawns levelClearEffectPrefab |
| README written | ✅ Done | SpaceMiner/README.md — update WebGL link after P5 build |
| Designer's Note written | ✅ Done | Included in README under Designer's Note section |
| WebGL build | ⬜ Not started | |
| APK build | ⬜ Not started | |
| GDD: level-configuration | ✅ Done | design/gdd/level-configuration.md |
| GDD: grid-system | ✅ Done | design/gdd/grid-system.md |
| GDD: ship-queue-system | ✅ Done | design/gdd/ship-queue-system.md |
| GDD: cargo-slot-system | ✅ Done | design/gdd/cargo-slot-system.md |
| GDD: mining-drone-system | ✅ Done | design/gdd/mining-drone-system.md |
| GDD: reward-system | ✅ Done | design/gdd/reward-system.md |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| WebGL audio blocked by browser autoplay policy | HIGH | Medium | Add "tap to start" overlay that triggers first AudioManager call |
| SFX sourcing takes longer than expected | MEDIUM | Low | Use Kenney.nl asset packs — all pre-categorized, free, instant download |
| APK build takes long due to Android SDK setup | MEDIUM | Low | Prioritize WebGL; APK is optional |
| Level 2 is too hard / too easy | LOW | High | Playtest 3× before building — adjust LevelData values if needed |

---

*Last updated: 2026-05-11 | Update this file as tasks complete*
