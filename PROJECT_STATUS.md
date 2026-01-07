# Dead Moroz 2025 - Project Status & Context

**Last Updated:** 07 January 2026

## Project Overview
**Dead Moroz 2025** is a runner style game (finite 12 levels) developed in Unity 6000.3 (URP).
**Goal:** Survive 12 months (levels) on a sleigh while dodging obstacles representing events of 2025.
**Mechanics:** 3-lane runner, 3-hit health system, increasing speed per month.
**Platforms:** Mobile, WebGL, Telegram Mini Apps.

## Architecture
### Managers (`Assets/Scripts/Managers/`)
*   **GameManager:** Core loop, state machine (Menu -> Game -> Win/Loss), speed scaling, month progression.
*   **EconomicManager:** Handles currency, loans, and monthly expenses. References `BankingScreenController`.
*   **UIManager:** Manages all UI panels (HUD, Main Menu, Pause, Game Over, Victory).
*   **EconomyManager:** *Legacy/Inactive* (superseded by `EconomicManager`).

### Player (`Assets/Scripts/Player/`)
*   **PlayerController:** Handles 3-lane movement (Lerp), speed, and New Input System integration.
*   **HealthSystem:** Manages player health (3 hits max).

### Obstacles (`Assets/Scripts/Obstacles/`)
*   **ObstacleSpawner:** Spawns text-block obstacles based on `MonthConfiguration`. Handles month transitions based on distance (Z-axis).
*   **ObstacleTextDisplay:** Displays the "event" text on the blocks.

## Gameplay Flow
1.  **Start:** `UIManager` triggers `GameManager.StartGame()`.
2.  **Run:** Player moves forward constantly. `ObstacleSpawner` spawns obstacles relative to player Z-position.
3.  **Progression:** As player crosses distance thresholds, `ObstacleSpawner` triggers `GameManager.AdvanceMonth()`.
4.  **Economy:** At month end, `EconomicManager` attempts to show a banking screen (via `BankingScreenController`) to handle expenses/loans.
5.  **End:** 
    *   **Victory:** Passing all 12 months.
    *   **Defeat:** 3rd collision triggers Game Over.

## Current State Observations
*   **UI:** Game Over screen "Passed Obstacles List" is fixed and functional.
*   **Economy Integration:** `EconomicManager` is wired up. The visual layer (`BankingScreenController`) is referenced but might need verification/implementation.
*   **Visuals:** `MonthVisualManager` calls are currently commented out in `GameManager` (visual environment changes inactive).
*   **Content:** Obstacle texts are likely sourced from `DeadMoroz_ObstacleTexts_Simple.txt`.