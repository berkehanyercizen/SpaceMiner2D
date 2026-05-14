# Adoption Plan

> **Generated**: 2026-05-11
> **Project phase**: Production (brownfield)
> **Engine**: Not configured (Unity 6000.4.5f1 confirmed in project, not yet set in CCGS)
> **Template version**: v1.0+

Work through these steps in order. Check off each item as you complete it.
Re-run `/adopt` anytime to check remaining gaps.

---

## Step 1: Fix Blocking Gaps

### 1.1 — Configure engine in technical-preferences.md
**Problem**: Every agent that spawns engine specialists reads `technical-preferences.md` first.
All fields are `[TO BE CONFIGURED]`, so agent routing is undefined — no specialist can be
dispatched for C# files, shaders, UI files, or scenes.

**Fix**: Run `/setup-engine unity 6` — this populates the engine, language, rendering,
physics, naming conventions, and file-extension routing table automatically.

**Time**: ~5 minutes
- [ ] `/setup-engine unity 6` run and `technical-preferences.md` updated

---

### 1.2 — Fix the engine reference pointer in docs/CLAUDE.md
**Problem**: `docs/CLAUDE.md` contains the line:
> `Current engine: see docs/engine-reference/godot/VERSION.md`

The Unity 6.3 LTS reference already exists at `docs/engine-reference/unity/VERSION.md`
(all modules present). Any agent reading this instruction file uses Godot docs for a Unity project.

**Fix**: Edit `docs/CLAUDE.md` — change the engine reference line from godot to unity.

**Time**: ~2 minutes
- [ ] `docs/CLAUDE.md` updated to point to `docs/engine-reference/unity/VERSION.md`

---

## Step 2: Fix High-Priority Gaps

### 2.1 — Create game-concept.md (design foundation)
**Problem**: The entire CCGS design pipeline starts from `design/gdd/game-concept.md`.
Without it, `/map-systems` has no stated concept to decompose, and `/review-all-gdds`
cannot validate systems against the design intent.

**Fix**: Run `/brainstorm` to create a formal game concept document, or write
`design/gdd/game-concept.md` manually from the existing implementation.

**Time**: 30 min
- [ ] `design/gdd/game-concept.md` created and passing `/design-review`

---

### 2.2 — Create systems-index.md
**Problem**: `/create-epics`, `/architecture-review`, and `/review-all-gdds` all
enumerate work by reading `design/gdd/systems-index.md`. It doesn't exist — these
skills cannot auto-discover what systems to process.

**Fix**: Run `/map-systems` after game-concept.md is written. This decomposes the
concept into individual systems and writes the index.

**Time**: 30–60 min (interactive)
- [ ] `design/gdd/systems-index.md` created with all 6+ systems listed

---

### 2.3 — Reverse-document core systems into GDDs
**Problem**: Zero GDDs exist. `/create-stories` requires GDDs with Acceptance Criteria.
`/architecture-review` traces GDD requirements. Without GDDs, no story generation,
no requirement tracing, no design review gates work.

SpaceMiner's 6 core systems (all already implemented in code):
1. Grid System (`GridManager`, `OreBlock`, `GridPathfinder`)
2. Ship Queue System (`CargoShip`, `ShipColumn`, `ShipQueueVisualizer`)
3. Slot System (`CargoSlot`, `SlotManager`, `SlotVisualizer`)
4. Drone System (`MiningDrone`, `DroneManager`)
5. Reward System (`RewardSystem`, `Modifier`)
6. UI / Game Flow (`GameManager`, `MainMenuController`, `PauseMenuController`)

**Fix**: For each system, run `/reverse-document design Assets/_Project/Scripts/Core/[SystemScript].cs`
Then run `/design-review design/gdd/[system].md` to validate.

**Time**: ~1 hour per system (6 systems = ~1 session)
- [ ] `design/gdd/grid-system.md` created and reviewed
- [ ] `design/gdd/ship-queue-system.md` created and reviewed
- [ ] `design/gdd/slot-system.md` created and reviewed
- [ ] `design/gdd/drone-system.md` created and reviewed
- [ ] `design/gdd/reward-system.md` created and reviewed
- [ ] `design/gdd/ui-game-flow.md` created and reviewed

---

### 2.4 — Note Unity source path mapping
**Problem**: CCGS skills that scan `src/` for code context find nothing — Unity source
lives at `Assets/_Project/Scripts/`, not the CCGS `src/` convention.

**Fix**: Root `CLAUDE.md` has been updated with a Unity path note clarifying the mapping.

**Time**: 5 min
- [ ] Root `CLAUDE.md` updated with Unity path note: `src/ → Assets/_Project/Scripts/Core/`

---

### 2.5 — Create control manifest
**Problem**: `/dev-story` reads `docs/architecture/control-manifest.md` to determine
which architecture layer a story belongs to and what rules apply. It doesn't exist —
dev-story will skip layer routing and produce unguarded implementation.

**Fix**: Write at least 3 ADRs first (architecture-decision ×3), then run
`/create-control-manifest`. The manifest is generated from accepted ADRs.

**Time**: 2 hours (ADRs) + 30 min (manifest generation)
- [ ] ADRs for singleton managers, LevelData ScriptableObject, drone lifecycle written
- [ ] `/create-control-manifest` run → `docs/architecture/control-manifest.md` created

---

## Step 3: Bootstrap Infrastructure

### 3a. Register existing requirements (creates tr-registry.yaml)
Run `/architecture-review` after GDDs exist — even if ADRs are sparse, this run bootstraps
the TR registry from your GDDs and writes stable TR-IDs that stories can reference.

**Time**: 1 session
- [ ] `docs/architecture/tr-registry.yaml` populated with TR-IDs

### 3b. Create sprint tracking file
Run `/sprint-plan` to formalize known issues as a backlog sprint. This creates
`production/sprint-status.yaml` which `/sprint-status` reads for reliable reporting.

**Time**: 30 min
- [ ] `production/sprint-status.yaml` created

### 3c. Set authoritative project stage
Run `/gate-check production` once GDDs and tests exist to formally assess readiness.
This writes `production/stage.txt` so phase auto-detection is reliable across sessions.

**Time**: depends on gate results
- [ ] `production/stage.txt` written

---

## Step 4: Medium-Priority Gaps

### 4.1 — Define naming conventions in technical-preferences.md
After `/setup-engine` runs, review the populated naming conventions and adjust them
to match the existing codebase style (PascalCase classes, camelCase fields, etc.).

**Time**: 15 min
- [ ] Naming conventions confirmed accurate to existing code

### 4.2 — Set performance budgets
Fill in Target Framerate (60fps mobile target), Frame Budget (~16.6ms), Draw Calls
ceiling, and Memory Ceiling based on target device tier.

**Time**: 15 min
- [ ] Performance budgets filled in `technical-preferences.md`

### 4.3 — Write 3 foundational ADRs
Document the key architectural decisions made during implementation:
- `adr-001-singleton-managers.md` — why singleton managers vs DI
- `adr-002-leveldata-scriptableobject.md` — why ScriptableObjects for level config
- `adr-003-drone-lifecycle.md` — why instantiate/destroy vs object pooling

Run `/architecture-decision` for each.

**Time**: 30 min each
- [ ] `adr-001-singleton-managers.md` written, Status: Accepted
- [ ] `adr-002-leveldata-scriptableobject.md` written, Status: Accepted
- [ ] `adr-003-drone-lifecycle.md` written, Status: Accepted

### 4.4 — Scaffold test framework
Run `/test-setup unity` to configure NUnit + Unity Test Runner, create the `tests/`
directory, and write first tests for GridPathfinder, RewardSystem, GameManager.

**Time**: 2–3 hours
- [ ] `tests/` directory scaffolded
- [ ] GridPathfinder: 3+ unit tests (impossible paths, single-block, full grid)
- [ ] RewardSystem: 2+ tests (modifier application, chest/key unlock sequence)
- [ ] GameManager: 2+ tests (win and lose state transitions)

### 4.5 — Populate production/stage.txt via /gate-check
- [ ] `/gate-check production` run (see Step 3c)

---

## Step 5: Optional Improvements

### 5.1 — Populate entity registry
After GDDs are written, populate `design/registry/entities.yaml` with ore colors,
ship types, and modifier definitions. Unlocks `/consistency-check`.

**Time**: 20 min
- [ ] Entities registered: OreColor ×5, CargoShip variants, Modifier types ×4

### 5.2 — Add production/review-mode.txt
Set review intensity. Recommended: `lean` for solo dev (phase gate reviews only).
- [ ] `production/review-mode.txt` created

### 5.3 — Add architecture-traceability.md
Generated automatically after `/architecture-review` completes a full run.
- [ ] Produced as part of Step 3a

---

## What to Expect from Existing Stories

No stories exist yet — the story-generation pipeline hasn't been run.
When stories are created via `/create-stories`, they will automatically include all
format fields (TR-IDs, manifest version, ADR references) from the start.

---

## Re-run

Run `/adopt` again after completing Steps 1 and 2 to verify all blocking and high gaps
are resolved. The new run will reflect the current state of the project.
