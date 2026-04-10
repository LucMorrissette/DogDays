# §15 Scripted Sequences

## Sequence Decisions

| Decision | Value | Rationale |
|---|---|---|
| **Sequence ownership** | Screen-owned orchestrators in `Systems/` | Keeps authored flow logic close to the screen that owns map state, input gating, dialog UI, and screen replacement. |
| **Sequence outputs** | Sequences emit pending requests instead of mutating screens directly | Preserves testability and keeps screen replacement, dialog start, event publication, and audio/UI side effects under screen control. |
| **Actor control contract** | Any actor a scripted sequence moves or poses implements `IScriptControllableActor` | Gives sequences one reusable API for positioning, facing, moving, and clearing movement state across players, followers, and future NPCs. |
| **Autonomous NPC control** | Patrolling NPCs additionally implement `IScriptControllableNpc` | Lets a sequence pause patrol or ambient behavior, apply a pose, move through furnished spaces with collision-aware scripted motion, and later resume autonomy without introducing story-specific flags into each NPC class. |
| **Scripted NPC travel** | When a map exposes an indoor nav graph, scripted patrol NPC beats should prefer authored A* routes to a named nav node over raw direct-to-point steering | Produces human-like movement around couches, tables, and other room obstacles instead of visible collision sliding. |
| **Route destination naming** | Scripted patrol-NPC beats should target stable nav-node names such as `entry`, `center`, or other authored staging labels | Keeps sequence code resilient to map layout tweaks and makes route intent readable in both code and Tiled. |
| **Directive storage** | Repeated anchors, poses, dialog payloads, and transitions live in focused definition classes backed by reusable position directives | Centralizes authored values without turning the sequence layer into a generic scripting engine. |
| **Movement helper** | Use `ScriptedActorMotion.MoveTowards(...)` for authored travel beats | Centralizes delta-time movement, arrival thresholds, zero-delta behavior, and end-of-step idle cleanup in one place. |
| **Dialog ownership** | Screens own `DialogSequence` and `DialogBoxRenderer`; sequences only queue `DialogScript` values and react to dismissals | Prevents narrative controllers from taking over UI/input concerns and keeps dialog rendering reusable across gameplay and cutscenes. |
| **Transition handoff** | Sequences request map/screen transitions explicitly, and the destination screen resumes from an authored phase enum or anchor | Avoids relying on zone-trigger collisions during scripted beats and keeps multi-map cutscenes deterministic. |
| **Autonomy freeze policy** | Pause ambient or patrol updates through the actor contract, not by sprinkling per-NPC special cases through the screen loop | Makes sequence control repeatable for future NPCs beyond Mom and Grandpa. |
| **Target sourcing** | Prefer named spawn points, nav-node names, or authored offsets from those anchors over raw room coordinates | Keeps sequence placement stable as maps evolve and avoids magic-number drift. |
| **Save/load boundary** | A sequence must either restart from a stable authored phase or explicitly model and restore its phase state | Prevents mid-sequence save corruption and makes restore expectations explicit. |
| **Update-loop discipline** | `Update()` may only advance sequence state and actor transforms; rendering, camera composition, and asset loads stay elsewhere | Keeps sequence code deterministic, testable, and compliant with the normal game-loop split. |
| **Testing rule** | Every new scripted sequence ships with unit tests for phase changes and pending outputs; multi-map sequences also get an integration or handoff test | Authored story flow is too brittle to leave untested once multiple sequences stack up. |

## Contract Catalog

| Contract / Helper | Role |
|---|---|
| `IScriptControllableActor` | Shared position/facing/movement API for scripted control of players, followers, and other movable scene actors. |
| `IScriptControllableNpc` | Extends actor control with autonomy enable/disable plus collision-aware scripted movement for patrol-driven or ambient NPCs. |
| `ScriptedPositionDirective` | Small authored data object for resolving a top-left world position from a named spawn-point anchor plus an offset. |
| `ScriptedActorPoseDirective` | Small authored data object for resolving a full actor pose from a scripted position directive plus facing. |
| `ScriptedNpcRoute` | Reusable A*-backed route follower for scripted patrol-NPC movement to named nav nodes. |
| `ScriptControllableActorExtensions` | Convenience helpers for applying a pose and freezing patrol NPCs for a scripted beat. |
| `ScriptedActorMotion` | Shared move-toward-target helper for authored cutscene movement. |
| `ScriptedPositionResolver` | Shared anchor resolver that turns scripted directives into world positions and actor poses. |
| `SummerIntroDefinition` | Example focused definition object that keeps one sequence's dialogs, anchors, offsets, and transition metadata together. |

## Implementation Checklist

| When adding a new scripted sequence... | Guideline |
|---|---|
| Choose ownership | Keep the sequence screen-owned unless the flow truly spans unrelated screens with shared runtime state. |
| Name the class | Use a focused `*Sequence` name that describes the authored beat or mechanic, not a generic `CutsceneManager`. |
| Define state | Use a small explicit enum for phases. Prefer stable authored phases over boolean combinations. |
| Control actors | Use `IScriptControllableActor` / `IScriptControllableNpc` instead of concrete actor-specific hooks whenever the action is reusable. |
| Store authored values | Keep repeated anchors, poses, dialogs, and transitions in a focused definition class rather than scattering literals through the screen and sequence. |
| Move actors | Use `ScriptedActorMotion.MoveTowards(...)` or another shared helper; for patrol NPC travel in furnished rooms, prefer `ScriptedNpcRoute` to a named nav node before falling back to direct steering. |
| Queue outputs | Expose pending dialog, transitions, and completion/event requests through consume-once methods. |
| Handle input | Let the owning screen decide which inputs remain active while the sequence runs. |
| Handle transitions | Transition only from an explicit sequence request, then re-enter the destination screen through a named sequence start state. |
| Handle NPC autonomy | Disable patrol or ambient autonomy before posing the NPC; re-enable it only after the sequence no longer needs control. |
| Handle NPC obstacles | If a screen-owned sequence bypasses the normal patrol update loop, clear the NPC's dynamic obstacle before scripted movement and restore it afterward so the NPC does not collide with its own stale footprint. |
| Handle saves | Either block saves mid-sequence, restart from a stable authored phase, or add explicit phase persistence and restore coverage. |
| Add tests | Cover phase progression, queued outputs, actor stop conditions, and any map or screen handoff. |
| Update docs | Update `docs/DESIGN.md` plus the relevant design sub-documents whenever the sequence introduces a new contract or pattern. |

## Patrol NPC Recipe

When another NPC needs a room-scale scripted walk, use this pattern:

1. Make the NPC implement `IScriptControllableNpc` with a `NavigationPosition` that matches the same foot-center space used by its patrol navigator.
2. Author named `NavNodes` at the staging spots a sequence cares about, such as `entry`, `center`, or a conversation mark near a table.
3. Add enough intermediate nodes and links around furniture, tight corners, and doorways that the desired route is explicit instead of implied by collision.
4. Store the destination node name in the sequence definition object, not inline in the screen.
5. In the owning screen, disable autonomy, build a `ScriptedNpcRoute` from `TiledWorldRenderer.NavGraph`, and pass it into the sequence.
6. During scripted updates, clear the NPC's dynamic obstacle before route movement and restore it immediately after.
7. Re-enable autonomy only after the sequence is done with the NPC.

Use direct point steering only when the walk is short, the path is unobstructed, or the map has no nav graph.

## Anti-Patterns To Avoid

| Anti-pattern | Why it hurts |
|---|---|
| Putting story flags directly into NPC classes | Couples reusable actor logic to one narrative beat and does not scale past the first few sequences. |
| Letting sequences draw directly | Mixes orchestration with rendering and makes them harder to test or reuse. |
| Driving transitions by physically colliding with normal zone triggers during a sequence | Makes handoff timing fragile and map-layout dependent. |
| Hardcoding all coordinates inline in the screen | Creates silent breakage when maps shift and makes authored intent impossible to read. |
| Steering a patrol NPC directly at a point through a furnished room | Leads to visible sliding, obstacle brushing, or getting stuck when a route to a named nav node would be deterministic and readable. |
| Using per-NPC bespoke freeze flags in screens | Multiplies branching as the NPC roster grows. |
| Saving in the middle of an unmodeled sequence | Leads to restore ambiguity and soft-lock risk. |
