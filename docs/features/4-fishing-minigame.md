# Feature 4 — Fishing Mini-Game (Side-View)

## Overview

A side-view fishing mini-game inspired by Breath of Fire 2. When the player is near water and presses Confirm, they are prompted "Fish Here? Yes/No". Choosing Yes transitions to a dedicated **FishingScreen** with its own side-view perspective, rendering, and (eventually) gameplay loop.

---

## New Pieces

| # | Piece | Type | Notes |
|---|---|---|---|
| 1 | **ConfirmationScreen** | New screen (reusable) | Transparent overlay with prompt text + Yes/No selection. Arrow-key navigation, Confirm to select, Cancel to dismiss. Reusable for future prompts. |
| 2 | **FishingScreen** | New screen | Self-contained side-view scene. Renders from a TMX map designed in Tiled. Fish silhouettes swim in the underwater area. |
| 2b | **SimpleTiledRenderer** | New renderer | Lightweight TMX loader for non-overworld screens. Supports single-image and collection-of-images tilesets. No terrain variants or water shader. |
| 2c | **FishSilhouette** | New entity | Animated fish with behavioral AI (idle, swim, dart, pause). Sprite atlas with 3 species × 6 animation frames. |
| 3 | **FishingZones** object layer | TMX + parser | Fishable areas authored as rectangles in Tiled maps. Parsed by `TiledWorldRenderer`, same pattern as `ZoneTriggers`. |
| 4 | **Interaction wiring** | GameplayScreen | Confirm + fishing zone intersection → push ConfirmationScreen → Yes → `ScreenManager.Replace(FishingScreen)`. |

---

## Design Decisions

### Screen Lifecycle: Replace (not Push)

Use `ScreenManager.Replace` to destroy the overworld when entering the fishing screen (consistent with existing zone transitions). On return, rebuild the overworld with the origin map and spawn point. Avoids dormant GameplayScreen in memory.

### Scene Rendering: TMX Map via SimpleTiledRenderer

The fishing scene is designed as a TMX map in Tiled (`Maps/FishingSpot.tmx`). A dedicated `SimpleTiledRenderer` loads and renders the map — it supports standard single-image tilesets (the normal Tiled workflow) without the overworld's terrain-variant system or water shader. The user paints the scene visually: sky, water surface, underwater area, shore/cliff, decorations.

A `SwimBounds` object layer in the TMX defines the rectangle where fish can swim. If absent, fish use the full map area.

### Fishing Zone Detection: TMX Object Layer

Author fishable areas as rectangles in a `FishingZones` object layer in Tiled (same pattern as `ZoneTriggers`). Checked on Confirm press (not on overlap). This gives designers control over exactly where fishing is allowed — not all water is fishable.

### Return Destination: Constructor Parameter

`FishingScreen` receives the origin map name + spawn ID via constructor. No hardcoded return destination.

---

## Phased Implementation

### Phase 1 — Scaffold (current target)

Goal: Full transition flow working end to end with placeholder visuals.

**FishingScreen renders:**
- Sky gradient at top
- Water surface line in the middle
- Dark blue underwater area below
- Ground/shore on the left side
- Player sprite standing on shore (reuse existing sprite, side-facing frame)
- "Press [Cancel] to return" text

**Behavior:**
- Cancel input → fade back to overworld (same fade pattern as zone transitions)
- No fish, no rod, no gameplay yet

**Implementation order:**
1. ConfirmationScreen — standalone, testable, no world dependency
2. FishingScreen scaffold — side-view placeholder, cancel-to-return
3. FishingZones in TMX — add fishable rectangles to StarterMap
4. Parse FishingZones in `TiledWorldRenderer`
5. Wire interaction in `GameplayScreen`
6. Tests — confirmation input, fishing zone detection, screen transitions

### Phase 2 — Fishing Gameplay (future)

- Cast animation, line/bobber rendering
- Fish entities swimming in the underwater area (simple AI: swim left→right, random depth)
- Bite detection, timing mini-game (button press window)
- Catch result screen / UI
- Fish inventory / catch log (triggers save persistence requirements)

### Phase 3 — Polish (future)

- Authored background art (sprite sheet or side-view TMX)
- Multiple fishing locations with different fish pools
- Rod upgrades, bait system
- Weather/time-of-day affecting fish availability

---

## Save Persistence

**Phase 1:** No new DTOs needed. The player transitions to the fishing screen and back — no mutable state to persist.

**Phase 2+:** When fish-catching, inventory, or rod state is added, save persistence kicks in:
- `SaveFishingData` DTO in `Data/Save/`
- Capture/restore in save mapper
- Round-trip tests

---

## Files Affected (Phase 1)

| File | Change |
|---|---|
| `Screens/ConfirmationScreen.cs` | New — reusable Yes/No prompt screen |
| `Screens/FishingScreen.cs` | New — side-view fishing scene (TMX-rendered) |
| `Entities/FishSilhouette.cs` | New — animated fish entity with swim/dart/idle/pause behaviors |
| `World/SimpleTiledRenderer.cs` | New — lightweight TMX renderer for non-overworld screens |
| `Screens/GameplayScreen.cs` | Add fishing zone check + ConfirmationScreen push |
| `World/TiledWorldRenderer.cs` | Parse `FishingZones` object layer |
| `Data/FishingZoneData.cs` | New — fishing zone rectangle DTO |
| `Content/Maps/StarterMap.tmx` | Add `FishingZones` object layer |
| `docs/DESIGN.md` | Update with new screens/systems |
| `docs/design/screens-input.md` | Add ConfirmationScreen + FishingScreen |
