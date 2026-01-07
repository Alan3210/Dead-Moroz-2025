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
*   **UI:** 
    *   Game Over screen "Passed Obstacles List" is fully functional.
    *   Banking Screen works with custom styled prefabs.
    *   TV Ticker text is properly masked.
*   **Economy Integration:** `EconomicManager` is wired up. `BankingScreenController` flow is polished (Animation + Sound).
*   **Visuals:** `MonthVisualManager` calls are currently commented out in `GameManager` (visual environment changes inactive).
*   **Content:** Obstacle texts are likely sourced from `DeadMoroz_ObstacleTexts_Simple.txt`.

## Next Steps Priorities
1.  **Visuals:** Enable `MonthVisualManager` for environment changes (Snow, Decorations).
2.  **Content:** Expand obstacle text pool.
3.  **Platform:** Verify Telegram Mini App integration.
