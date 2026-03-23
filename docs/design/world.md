# §20 World & Tilemap

## World Classes

| Class | Description |
|---|---|
| `IMapCollisionData` | World collision query contract for blocked-tile checks using world-space rectangles. |
| `TiledWorldRenderer` | TMX/TSX-backed world renderer that draws ordered tile layers, routes `Water/*` layers through the water pass, aggregates tile-property collision across all layers, and exposes TMX object-layer prop placements. Exposes a `ColliderBounds` property and `LoadColliderBounds()` method to parse pure geometric rectangles (objects without a `gid`) from a `"Colliders"` object layer as world-space rectangles. |
| `WorldCollisionMap` | Collision aggregator that combines terrain blockers with additional placed obstacle bounds. |

*(Add entries as world/tilemap classes are created — TileMap, TileMapRenderer, etc.)*

## Prop Collision Strategy

Collision boxes for world props are defined **in code** as part of the entity class, not hand-placed in the TMX `Colliders` layer. This eliminates per-instance manual placement and ensures every instance of a prop type gets an identical, correct collision box automatically.

### How it works

1. **Entity class** (`Tree`, `Cabin`) accepts a `localCollisionBox` `Rectangle` in its constructor. This rectangle is relative to the sprite's top-left origin.
2. **`PropFactory`** holds `static readonly Rectangle` constants for each prop type's collision box (e.g., `PineTreeCollisionBox`, `BirchTreeCollisionBox`, `CozyCabinCollisionBox`).
3. **`GameplayScreen.LoadContent()`** calls `PropFactory.CreateTrees()` / `PropFactory.CreateCabins()` with the appropriate collision box constant, then merges collision bounds via `PropFactory.GetTreeCollisionBounds()` / `PropFactory.GetCabinCollisionBounds()` into the `WorldCollisionMap`.

### TMX Colliders layer

The TMX `Colliders` object layer is reserved for **terrain and world-boundary colliders only** (e.g., ground/water borders). Do not add prop-specific collision rectangles to this layer.

### Adding a new solid prop type

1. Define a `static readonly Rectangle` in `PropFactory` describing the collision area relative to the sprite's top-left corner.
2. Use `PropFactory.CreateTrees()` or `PropFactory.CreateCabins()` (or create a new entity type following the `Tree`/`Cabin` pattern with a `CollisionBounds` property).
3. Add a `GetXxxCollisionBounds()` helper to `PropFactory`.
4. Merge the collision bounds in `GameplayScreen.LoadContent()` via `PropFactory.MergeRectangleArrays()`.

<!-- Example format:
| `TileMap` | Core tile data, collision queries, runtime modifications. |
| `TileMapRenderer` | Tile rendering with per-layer support. |
-->
