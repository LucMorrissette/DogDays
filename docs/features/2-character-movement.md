# Feature 2 - Character Movement

## Goal

Introduce first-pass player control with a placeholder solid-color block that moves in world space using action-based input and delta time.

## Why This Feature Exists

After proving tilemap rendering, we need a controllable actor to validate update-loop movement, camera follow behavior, and game-feel iteration loops.

## Player-Facing Outcome

When the game runs, the player can move a visible colored block with WASD or arrow keys across the map. The camera follows the block while remaining clamped to map bounds.

## Requirements

1. Add a minimal player entity with world position, size, speed, and map-bounds clamping.
2. Drive movement through `IInputManager` actions (`MoveUp`, `MoveDown`, `MoveLeft`, `MoveRight`).
3. Use delta time for movement and normalize diagonal input.
4. Render the player as a solid color rectangle in the world draw pass.
5. Make the camera follow the player center each frame.
6. Add unit and integration tests covering movement and clamping behavior.

## Non-Goals

- Sprite animation
- Combat interactions
- Save/load persistence for this temporary prototype entity

## Acceptance Criteria

1. Player block is visible on top of the tilemap.
2. Holding movement keys moves at consistent speed in all directions.
3. Camera follow keeps the player near viewport center except when clamped by map edges.
4. Build and tests pass.
