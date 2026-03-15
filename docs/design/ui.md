# §13 UI & HUD

## UI Decisions

| Decision | Value | Rationale |
|---|---|---|
| **UI logic location** | Separate Renderer classes | UI code stays out of entities and screens. |
| **Rendering pass** | Separate SpriteBatch without camera transform | UI is screen-space, not world-space. |

*(Add entries as UI patterns are established — state updates, dialogue system, inventory interaction, etc.)*

## UI Classes

| Class | Description |
|---|---|

*(Add entries as UI classes are created — HudRenderer, DialogueBoxRenderer, etc.)*

<!-- Example format:
| `HudRenderer` | Renders main HUD (score, health, etc.). |
| `DialogueBoxRenderer` | Renders dialogue UI box, text, and choices. |
-->
