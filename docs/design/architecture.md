# §9 Architecture & Patterns

| Decision | Value | Rationale |
|---|---|---|
| **Entity design** | Composition over inheritance | No `GameObject` base class. Entities are built from focused components. |
| **System decoupling** | `GameEventBus` + local C# events | Gameplay systems keep focused local events, while cross-cutting progression flows publish lightweight `GameEvent` payloads through a shared bus. |
| **Game1 responsibility** | Wire up systems, delegate to ScreenManager | `Game1` stays thin — no gameplay logic. |
| **Dependency injection** | `Game.Services` + constructor injection | Avoids singletons; keeps classes testable. |
| **Screen management** | Stack-based ScreenManager | Screens push/pop (gameplay, pause, inventory). Topmost screen receives input. |
| **Input abstraction** | IInputManager interface | Gameplay code never touches `Keyboard.GetState()` directly. Testable via fakes. |
| **XNA-native first** | Prefer MonoGame/XNA built-in types and patterns | Custom solutions only when XNA doesn't provide what's needed. |
| **Navigation / steering / collision split** | Navigation (route selection) → Steering (route following) → Collision (step validation) | Each layer has a single job: nav picks reachable routes, steering follows them, collision resolves each step. Prevents mixing pathfinding with physics. |
| **Quest progression** | Session-owned `QuestManager` reacts to `GameEventBus` payloads | Quest state survives `GameplayScreen` replacement while quest objectives remain decoupled from NPC, combat, and zone code. |
| **Quest-reactive NPC dialog** | Main-story NPC hints are authored on the quest definition and resolved through `QuestNpcDialogRegistry` using stable NPC ids | Keeps story bark content in quest data, lets multiple NPCs react to the same quest, and avoids scattering quest checks through `GameplayScreen` or NPC classes. |
| **Story opener sequencing** | Screen-owned `SummerIntroSequence` drives dialog, scripted walking, and the cabin-to-bedroom handoff, then reports completion through `GameEventBus` story-beat payloads | Keeps authored intro flow out of general-purpose NPC classes while preserving event-driven quest completion across screen replacement. |
| **Script-controlled actors** | Reusable scripted sequences operate on `IScriptControllableActor`, while autonomous patrol NPCs add `IScriptControllableNpc` to pause and later resume autonomy from the new location | Keeps scripted-sequence control reusable across players, followers, and NPC types without baking story-specific flags into each implementation. |
| **Scripted patrol-NPC routing** | Patrol NPCs should use `ScriptedNpcRoute` plus named `IndoorNavGraph` nodes for room-scale story beats when a furnished map exposes nav data | Produces readable, human-like paths around couches, tables, and door frames instead of direct-point steering plus collision sliding. |
| **Directive-backed authored beats** | Repeated sequence anchors, poses, dialogs, and transitions live in focused definition classes plus reusable anchor directives, while phase progression stays in code | Removes brittle literals from screen orchestration without committing the project to a full scripting engine. |

*(Add entries as new architectural patterns emerge: zone systems, object pooling, factories, sequencers, trigger zones, etc.)*
