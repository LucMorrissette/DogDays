# §14 Audio

| Decision | Value | Rationale |
|---|---|---|
| **Audio system access** | Game Service (`IAudioManager`) | Centralized, globally accessible, testable via fake. |
| **Asset loading** | Centralized in AudioManager.LoadContent | Screens use logical names, not file paths. |

*(Add entries as audio patterns are established — SFX triggering, music playback, ambient sound, volume hierarchy, etc.)*
