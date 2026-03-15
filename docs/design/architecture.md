# §9 Architecture & Patterns

| Decision | Value | Rationale |
|---|---|---|
| **Entity design** | Composition over inheritance | No `GameObject` base class. Entities are built from focused components. |
| **System decoupling** | Static Event Hub (`GameEvents`) | For global, fire-and-forget notifications (score change, entity death, etc.). |
| **Game1 responsibility** | Wire up systems, delegate to ScreenManager | `Game1` stays thin — no gameplay logic. |
| **Dependency injection** | `Game.Services` + constructor injection | Avoids singletons; keeps classes testable. |
| **Screen management** | Stack-based ScreenManager | Screens push/pop (gameplay, pause, inventory). Topmost screen receives input. |
| **Input abstraction** | IInputManager interface | Gameplay code never touches `Keyboard.GetState()` directly. Testable via fakes. |
| **XNA-native first** | Prefer MonoGame/XNA built-in types and patterns | Custom solutions only when XNA doesn't provide what's needed. |

*(Add entries as new architectural patterns emerge: zone systems, object pooling, factories, sequencers, trigger zones, etc.)*
