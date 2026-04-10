# §20 World & Tilemap

## World Classes

| Class | Description |
|---|---|
| `IMapCollisionData` | World collision query contract for blocked-tile checks using world-space rectangles. |
| `TiledWorldRenderer` | TMX/TSX-backed world renderer that draws ordered tile layers, routes `Water/*` layers through the water pass, aggregates tile-property collision across all layers, exposes TMX object-layer prop placements, and renders non-`propType` tile objects as a dedicated top overlay. Also exposes `ColliderBounds`, `ZoneTriggers`, `SpawnPoints`, and an optional `IndoorNavGraph` parsed from dedicated TMX object layers so gameplay can stay editor-driven. |
| `WorldCollisionMap` | Collision aggregator that combines terrain blockers with additional placed obstacle bounds. |
| `IndoorNavNode` | A single navigable point in an indoor map with an id, position, optional name, and tags. Loaded from TMX `NavNodes` object layer point objects. |
| `IndoorNavLink` | Bidirectional connection between two `IndoorNavNode` instances. Loaded from TMX `NavLinks` object layer polyline objects with endpoint-to-node proximity matching. |
| `IndoorNavGraph` | Collection of `IndoorNavNode` and `IndoorNavLink` with adjacency-list structure. Provides nearest-node lookup and A* route computation. Nullable on `TiledWorldRenderer` — only present for maps that author nav data. |
| `SimpleTiledRenderer` | Lightweight TMX/TSX renderer for the fishing mini-game. Draws all tile layers via a single `Draw(Matrix)` call or per-layer via `DrawLayer(string, Matrix)`. Exposes `GetObjectRectangles(layerName)` and `GetObjectPolygons(layerName)` for parsing zone/swim-area object layers into `Rectangle[]` / `PolygonBounds[]`. No terrain-variant system; no water-pass routing. Implements `IDisposable`. |

*(Add entries as world/tilemap classes are created — TileMap, TileMapRenderer, etc.)*

## Prop Collision Strategy

Collision boxes for world props are defined **in code** as part of the entity class, not hand-placed in the TMX `Colliders` layer. This eliminates per-instance manual placement and ensures every instance of a prop type gets an identical, correct collision box automatically.

### How it works

1. **Entity class** (`Tree`, `Cabin`) accepts a `localCollisionBox` `Rectangle` in its constructor. This rectangle is relative to the sprite's top-left origin.
2. **`PropFactory`** holds `static readonly Rectangle` constants for each prop type's collision box (e.g., `PineTreeCollisionBox`, `BirchTreeCollisionBox`, `CozyCabinCollisionBox`).
3. **`GameplayScreen.LoadContent()`** calls `PropFactory.CreateTrees()` / `PropFactory.CreateCabins()` with the appropriate collision box constant, then merges collision bounds via `PropFactory.GetTreeCollisionBounds()` / `PropFactory.GetCabinCollisionBounds()` into the `WorldCollisionMap`.

### TMX Colliders layer

The TMX `Colliders` object layer is reserved for **terrain and world-boundary colliders only** (e.g., ground/water borders). Do not add prop-specific collision rectangles to this layer.

## Decorative Tile-Object Overlays

TMX object layers may also contain **tile objects without a `propType` property**. These are treated as decorative overlays rather than gameplay props.

### How it works

1. `TmxObjectLoader` skips tile objects whose global tile id maps to TSX `propType` metadata, because those become screen-owned prop entities.
2. Remaining tile objects are parsed into decorative placements with their authored size, position, and flip flags preserved.
3. `TiledWorldRenderer` renders those decorative tile objects in a dedicated overlay pass after world entities.

### When to use this path

- Use it for cutaway wall borders, room trim, non-interactive decorative overlays, and similar editor-authored visuals.
- Do not use it for anything that needs collision, saveable runtime state, or behavior. Those still belong on the `propType` entity path.

## Zone Transition Authoring

Zone-to-zone travel is authored entirely in TMX object layers rather than terrain tile metadata.

### `ZoneTriggers` layer

- Use plain rectangle objects with no `gid`.
- Required property: `targetMap` — destination TMX content asset name (for example `Maps/WoodsBehindCabin`).
- Optional property: `targetSpawnId` — destination spawn point name. Defaults to `default` when omitted.

When the player overlaps a trigger rectangle, `GameplayScreen` replaces itself with a fresh `GameplayScreen` for the destination map. This keeps map loading atomic and avoids carrying stale world state across zones.

### `SpawnPoints` layer

- Use named point objects with no `gid`.
- The TMX object `name` is the spawn identifier.
- `GameplayScreen` looks up the requested spawn id and falls back to map centre if no matching spawn exists.

This pairing gives each exit its own per-instance destination data without encoding map links into shared tile definitions.

## Indoor Navigation Authoring

Indoor maps can optionally include authored navigation data for NPC pathfinding via two dedicated TMX object layers.

### `NavNodes` layer

- Use **point objects** (no width/height) at walkable positions.
- Each object's Tiled `id` becomes the node identifier.
- The TMX object `name` is an optional human-readable label.
- Custom property: `tags` (string, comma-separated) — optional behaviour hints (e.g. `idle`, `lounge`, `entry`).
- Object `type` should be set to `nav-node` for editor clarity.

### `NavLinks` layer

- Use **polyline objects** with exactly two points connecting two `NavNode` positions.
- Links are resolved to node IDs by proximity (endpoints must be within 2 pixels of a node position).
- Links are **bidirectional** — each link creates edges in both directions.
- Explicitly authored links prevent hidden routing through walls.

### Scripted Route Authoring

For room-scale story beats, treat nav data as both patrol data and scripted-route data.

- Add stable `NavNode` names for sequence destinations such as `entry`, `center`, `hearth`, or other conversation staging spots.
- Keep those names stable once sequence code references them.
- Place intermediate nodes around couches, tables, counters, and narrow door frames so A* produces the path a player would expect to see.
- Prefer one or two extra corner nodes over relying on collision sliding to shape the final path.
- Do not place nodes inside furniture footprints or so close to obstacles that the NPC foot bounds must scrape them.
- If a route should clearly approach from one side of the room, author links that make that approach explicit instead of leaving multiple equally good paths.

The goal is not just reachability. The goal is readable, human-looking movement for both ambient patrols and scripted sequences.

Maps without nav data work unchanged — `TiledWorldRenderer.NavGraph` is `null` in that case.

### Adding a new solid prop type

1. Define a `static readonly Rectangle` in `PropFactory` describing the collision area relative to the sprite's top-left corner.
2. Use `PropFactory.CreateTrees()` or `PropFactory.CreateCabins()` (or create a new entity type following the `Tree`/`Cabin` pattern with a `CollisionBounds` property).
3. Add a `GetXxxCollisionBounds()` helper to `PropFactory`.
4. Merge the collision bounds in `GameplayScreen.LoadContent()` via `PropFactory.MergeRectangleArrays()`.

### Adding a decorative state-variant prop

1. Add each authored sprite variant to `Content.mgcb` and expose it through `Tilesets/Props.tsx` with a distinct `propType` value.
2. Create a focused `IWorldProp` implementation when the variants belong to one concept instead of routing the behavior through a generic catch-all prop.
3. Add a factory helper in `PropFactory` that maps each `propType` variant to the correct texture while keeping placement data editor-driven.
4. If gameplay needs to react to nearby actors, update the focused prop from `GameplayScreen.Update()` using world-space bounds and tile-sized thresholds rather than querying input directly inside the prop.
5. Draw the resulting prop array from `GameplayScreen.DrawWorldEntities()` so the variants participate in the normal world sort order.

<!-- Example format:
| `TileMap` | Core tile data, collision queries, runtime modifications. |
| `TileMapRenderer` | Tile rendering with per-layer support. |
-->
