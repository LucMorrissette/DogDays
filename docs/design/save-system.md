# §18 Save & Persistence

| Decision | Value | Rationale |
|---|---|---|
| **Save abstraction** | `ISaveGameService` interface | Decoupled from file system; testable via fakes. |
| **Save format** | JSON | Human-readable, easy to debug during development. |
| **Save format versioning** | `SaveGameData.CurrentVersion` integer | Enables explicit gating and migration when schema changes. |
| **Capture/restore pattern** | `SaveGameMapper` with deterministic capture/apply | Single source of truth for serialization logic. |

*(Add entries as save system evolves — slot count, trigger mechanism, atomic writes, persisted state inventory, etc.)*

## Persisted State

*(List what's persisted as the game grows — player position, health, inventory, world state, etc.)*

## Explicitly Not Persisted

*(List things intentionally excluded from the save — particle effects, screen shake, etc.)*
