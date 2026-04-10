# Feature 7 - Missing Gnome Quest

## Goal

Add the next authored cottage story beat so leaving the house after the summer intro triggers Mom's warning about the missing garden gnome and unlocks the next quest.

## Why This Feature Exists

The summer intro establishes the boys and unlocks free roaming, but the game needs a clear next objective before the player heads into the forest. This feature turns the cottage exit into a short authored interruption that gives the shed and the missing gnome immediate narrative purpose.

## Player-Facing Outcome

After the first-day-of-summer intro is complete, attempting to leave the cottage causes Mom to come over and ask whether the boys have seen Gnome Chompsky. She explains that the shed key was hidden inside the missing gnome, so the shed will stay locked until it is found. That beat unlocks the quest `Search for Gnome Chompsky`, whose first objective is `Search for the missing gnome`. Entering the forest completes that objective.

## Requirements

1. The sequence only triggers after `First Day of Summer` is complete.
2. The sequence triggers from the cabin-indoor exit to outdoors before the player transitions out.
3. Mom must walk over to the boys before the dialog begins.
4. The dialog must mention the missing garden gnome, the shed key hidden inside it, and the possibility that it was stolen.
5. Finishing the sequence must publish a story beat that starts the next quest.
6. The unlocked quest title must be `Search for Gnome Chompsky`.
7. The first objective text must be `Search for the missing gnome`.
8. Entering `Maps/WoodsBehindCabin` must complete that objective.

## Non-Goals

- Persisting the sequence mid-dialog or mid-walk
- Adding a full branching dialog system for Mom
- Unlocking the shed itself in this feature

## Acceptance Criteria

1. After the summer intro, the first attempt to leave the cottage starts Mom's missing-gnome warning sequence instead of transitioning outdoors immediately.
2. Finishing the dialog starts `Search for Gnome Chompsky` and shows it in the quest UI.
3. Subsequent exit attempts do not replay the sequence once the quest has started.
4. Entering the forest zone completes the quest objective.
5. Focused tests and verification build pass.