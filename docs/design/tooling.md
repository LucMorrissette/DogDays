# §27 Technical Gotchas · §28 Dev Assets & Tooling · §29 Developer Tools

## Technical Gotchas

| Gotcha | Context | Resolution |
|---|---|---|
| `RenderTarget2D` defaults to `DiscardContents` | Switching between render targets silently discards previous contents | Use `RenderTargetUsage.PreserveContents` for any RT that gets switched away from and back to within a single frame. |

*(Add entries as gotchas are discovered.)*

## Dev Assets & Tooling

| Decision | Value | Rationale |
|---|---|---|

*(Add entries for placeholder art generation, asset scripts, content pipeline tooling, etc.)*

## Developer Tools

| Tool | Toggle Action | Key | Description |
|---|---|---|---|

*(Add entries for in-game debug tools — tile inspector, collision visualizer, etc.)*
