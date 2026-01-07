# Dead Moroz 2025 - Project Status & Context

**Last Updated:** 07 January 2026

## Project Overview
**Dead Moroz 2025** is a runner style game (finite 12 levels) developed in Unity 6000.3 (URP).
**Goal:** Survive 12 months (levels) on a sleigh while dodging obstacles representing events of 2025.
**Mechanics:** 3-lane runner, 3-hit health system, increasing speed per month.
**Platforms:** Mobile, WebGL, Telegram Mini Apps.

## Architecture
### Managers (`Assets/Scripts/Managers/`)
*   **GameManager:** Core loop, state machine. Manages `TVController` activation and Month progression.
*   **EconomicManager:** Handles currency, loans, and monthly expenses.
*   **UIManager:** Manages all UI panels (HUD, Main Menu, Pause, Game Over, Victory).
*   **BankingScreenController:** Controls the end-of-month financial summary.
    *   *Feature:* Triggers `TVController.LiftTV()` to clear screen space when active.

### Player (`Assets/Scripts/Player/`)
*   **PlayerController:** Handles 3-lane movement (Lerp), speed, and New Input System integration.
*   **HealthSystem:** Manages player health (3 hits max).

### Obstacles (`Assets/Scripts/Obstacles/`)
*   **ObstacleSpawner:** Spawns text-block obstacles based on `MonthConfiguration`. Handles month transitions based on distance (Z-axis).
*   **ObstacleTextDisplay:** Displays the "event" text on the blocks.

### UI & Visuals (`Assets/Scripts/UI/`)
*   **TVController:** Manages the animated TV companion.
    *   *Logic:* Activates on Game Start.
    *   *Animation:* Lifts up/down during Banking phase (Supports 3D transform).
    *   *Audio:* Has start, lift, and lower SFX hooks.
*   **PassedObstaclesList:** Displays a scrollable history of "events" survived on the Game Over screen.

## Gameplay Flow
1.  **Start:** `UIManager` triggers `GameManager.StartGame()`. TV activates.
2.  **Run:** Player moves forward. `ObstacleSpawner` spawns obstacles.
3.  **Progression:** `ObstacleSpawner` triggers `GameManager.AdvanceMonth()`.
4.  **Economy:** At month end, `BankingScreenController` appears.
    *   **Transition:** TV lifts up, Banking UI fades in (Slow motion).
    *   **Action:** User pays expenses or takes microloan.
    *   **Resume:** TV lowers, game speed restores.
5.  **End:** 
    *   **Victory:** Passing all 12 months.
    *   **Defeat:** 3rd collision triggers Game Over (shows Passed Obstacles).

## Current State Observations

### WebGL & Vertical Adaptation (New)
*   **Responsive Vertical Export:** 
    *   Implemented `VerticalAspectAdapter.cs` to automatically adjust camera distance when running in portrait mode (9:16), keeping the full road width visible.
    *   **Custom WebGL Template (`ResponsiveVertical`):** 
        *   Forces 9:16 aspect ratio via JavaScript resizer (pillarboxing on wide screens).
        *   Fixes WebGL 0x0 texture crash by ensuring canvas has valid dimensions before Unity initializes.
        *   Includes "Tap to Start" overlay to comply with browser AudioContext policies.
*   **Graphics Optimization:**
    *   Switched WebGL default quality to **"PC" Profile** (Scale 1.0 vs Mobile 0.8).
    *   Enabled **4x MSAA** (Anti-Aliasing) for crisp edges.
    *   Enabled `window.devicePixelRatio` support for high-DPI (Retina/4K) rendering.
*   **Deployment:** Configured for itch.io with specific embed settings (540x960 viewport, Portrait, Mobile Friendly).

### UI & Core Features
*   **UI:** 
    *   **Controls Text:** Fully configurable (Delay, Duration, Fade).
    *   Game Over screen "Passed Obstacles List" is functional.
    *   Banking Screen fully integrated (Animation + Sound).
    *   **TV Companion:** Added looping ambience sound.
*   **Visuals:** 
    *   **Intro Sequence:** "Falling Sky" animation for level elements implemented with bounce physics.
    *   **Dynamic Skybox/Fog:** `MonthVisualManager` handles visual transitions per month.
*   **Content:** Obstacle texts sourced from `DeadMoroz_ObstacleTexts_Simple.txt`.

## Next Steps Priorities
1.  **Content:** Expand obstacle text pool.
2.  **Platform:** Verify Telegram Mini App integration.
3.  **Polishing:** Fine-tune the speed increase curve and obstacle spawn frequency.