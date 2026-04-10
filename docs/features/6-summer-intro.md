# Feature 6 - Summer Intro

## Goal

Replace the placeholder opening with a real first quest that starts in the bedroom, sends the player to Mom, introduces the companion, and unlocks the party only after the opening scene resolves.

## Why This Feature Exists

The project already had quest, dialog, and follower systems, but the opening loop was still placeholder content that immediately spawned the companion and pushed the player toward Grandpa and combat beats. This feature establishes the game's actual narrative starting point and makes party availability match the story.

## Player-Facing Outcome

Selecting New Game now spawns the player alone in the bedroom. The first quest tells them to go talk to Mom. After that conversation, the companion walks into the cottage, talks about spending the summer by the river, and the boys head back into the bedroom for a short final exchange. Completing that beat finishes Quest 1 and permanently enables the companion follower.

## Requirements

1. New Game must reset session quest/runtime state before loading the opener.
2. New Game must begin on `Maps/CabinBedroom` with no visible companion.
3. The only starting quest must be the opening summer-vacation quest.
4. The tracked opening objective text must read `Go talk to Mom.`
5. Talking to Mom for the first time must play the authored summer-vacation dialog instead of generic random bark text.
6. The companion must appear only during the authored entrance beat and must not be usable in follower-dependent interactions before the opener finishes.
7. The cabin scene must hand off into a bedroom return scene before the quest completes.
8. Finishing the bedroom chat must publish a story beat that completes Quest 1 and unlocks the companion.
9. Save/load and fresh New Game flows must derive companion availability from quest state rather than a hardcoded spawn toggle.
10. Trying to leave the cottage for outdoors before the opener completes must block the exit and remind the player to talk to Mom first.

## Non-Goals

- Building a reusable full cutscene editor or timeline system
- Reworking Mom or Grandpa into bespoke story controllers
- Persisting mid-cutscene state across saves

## Acceptance Criteria

1. Starting a new game always places the player alone in the bedroom.
2. The pause tracker shows only the opening quest and objective.
3. The companion is absent from update, draw, couch, and watercraft paths until the opening quest completes.
4. Talking to Mom triggers the authored intro instead of the previous placeholder quest path.
5. The companion walks in, the scene transitions back to the bedroom, and the final chat completes the quest.
6. After the quest finishes, the companion follows normally on subsequent gameplay screens.
7. Walking into the cabin's outdoor exit before the opener completes keeps the player inside and shows a reminder to talk to Mom first.
8. Focused tests and verification build pass.
