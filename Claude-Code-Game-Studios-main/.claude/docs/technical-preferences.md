# Technical Preferences

<!-- Populated by /setup-engine. Updated as the user makes decisions throughout development. -->
<!-- All agents reference this file for project-specific standards and conventions. -->

## Engine & Language

- **Engine**: Unity 6 (6000.4.5f1)
- **Language**: C#
- **Rendering**: Universal Render Pipeline (URP) 2D
- **Physics**: Unity Physics 2D (built-in)

## Input & Platform

<!-- Written by /setup-engine. Read by /ux-design, /ux-review, /test-setup, /team-ui, and /dev-story -->
<!-- to scope interaction specs, test helpers, and implementation to the correct input methods. -->

- **Target Platforms**: Mobile (Android / iOS), PC (secondary — editor/desktop fallback)
- **Input Methods**: Touch (primary), Keyboard/Mouse (PC fallback)
- **Primary Input**: Touch
- **Gamepad Support**: None
- **Touch Support**: Full
- **Platform Notes**: Mobile-first. All interactions are tap-based; no hover-only states. UI must
  function correctly at 9:16 portrait and 16:9 landscape. Minimum touch target size: 44px.

## Naming Conventions

- **Classes**: PascalCase (e.g., `GridManager`, `CargoShip`)
- **Variables**: Private fields: `_camelCase` (e.g., `_moveSpeed`); public fields/properties: PascalCase
- **Signals/Events**: C# events in PascalCase (e.g., `OnBlockMined`, `OnSlotFilled`)
- **Files**: PascalCase matching class name (e.g., `GridManager.cs`, `LevelData.cs`)
- **Scenes/Prefabs**: PascalCase matching root GameObject (e.g., `Level1.unity`, `TinyShip1.prefab`)
- **Constants**: PascalCase or UPPER_SNAKE_CASE (e.g., `MaxSlotCount`, `DEFAULT_COLUMN_COUNT`)

## Performance Budgets

- **Target Framerate**: [TO BE CONFIGURED — recommend 60fps for mobile]
- **Frame Budget**: [TO BE CONFIGURED — 16.6ms at 60fps]
- **Draw Calls**: [TO BE CONFIGURED — recommend ≤100 for mobile URP 2D]
- **Memory Ceiling**: [TO BE CONFIGURED — recommend ≤512MB for mid-tier Android]

## Testing

- **Framework**: NUnit via Unity Test Runner (Edit Mode + Play Mode)
- **Minimum Coverage**: [TO BE CONFIGURED]
- **Required Tests**: Balance formulas, gameplay systems (pathfinder, reward logic, state transitions)

## Forbidden Patterns

<!-- Add patterns that should never appear in this project's codebase -->
- Hardcoded scene name strings — use `static class SceneNames` with `const string` entries
- Hardcoded animator state strings — cache `Animator.StringToHash()` results in `Awake()`
- `Time.timeScale` set outside of `GameManager` — all time control must go through GameManager
- Creating `new Texture2D(...)` without a corresponding `Destroy()` in `OnDestroy()`

## Allowed Libraries / Addons

<!-- Add approved third-party dependencies here — only when actively integrated -->
- Unity New Input System (com.unity.inputsystem) — active
- TextMesh Pro — active
- Universal Render Pipeline (URP) — active
- 30 Sci-fi Space Tracks Music Pack — active (audio assets)

## Architecture Decisions Log

<!-- Quick reference linking to full ADRs in docs/architecture/ -->
- [No ADRs yet — use /architecture-decision to create one]

## Engine Specialists

<!-- Written by /setup-engine when engine is configured. -->
<!-- Read by /code-review, /architecture-decision, /architecture-review, and team skills -->
<!-- to know which specialist to spawn for engine-specific validation. -->

- **Primary**: unity-specialist
- **Language/Code Specialist**: unity-specialist (C# review — primary covers it)
- **Shader Specialist**: unity-shader-specialist (Shader Graph, HLSL, URP/HDRP materials)
- **UI Specialist**: unity-ui-specialist (UI Toolkit UXML/USS, UGUI Canvas, runtime UI)
- **Additional Specialists**: unity-dots-specialist (ECS, Jobs, Burst — not currently in use), unity-addressables-specialist (asset loading, memory management)
- **Routing Notes**: Invoke primary for architecture and general C# code review. Invoke shader specialist for any rendering and visual effects work. Invoke UI specialist for all interface implementation. Invoke Addressables specialist if asset management systems are added.

### File Extension Routing

<!-- Skills use this table to select the right specialist per file type. -->

| File Extension / Type | Specialist to Spawn |
|-----------------------|---------------------|
| Game code (.cs files) | unity-specialist |
| Shader / material files (.shader, .shadergraph, .mat) | unity-shader-specialist |
| UI / screen files (.uxml, .uss, Canvas prefabs) | unity-ui-specialist |
| Scene / prefab / level files (.unity, .prefab) | unity-specialist |
| Native extension / plugin files (.dll, native plugins) | unity-specialist |
| General architecture review | unity-specialist |
