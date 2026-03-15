# River Rats

A new Windows-native MonoGame project scaffolded for PC-first development.

## Current stack
- C#
- .NET 9 Windows desktop target
- MonoGame `WindowsDX`

## Why this template
This project uses the Windows desktop MonoGame template so development, build, and packaging stay on the native Windows toolchain. That is a better fit for a future Steam release and a later PC Game Pass / Xbox ecosystem submission path.

## Project layout
- `RiverRats.slnx` — solution file
- `src/RiverRats.Game` — main MonoGame game project
- `.github/copilot-instructions.md` — workspace checklist

## Prerequisites
- Windows
- .NET SDK installed
- MonoGame templates installed for the Windows `dotnet` CLI

## Build
Use the Windows CLI:
- `dotnet build RiverRats.slnx`

## Run
- `dotnet run --project src/RiverRats.Game/RiverRats.Game.csproj`

## Next recommended milestones
1. Replace the starter loop with a real screen/state system.
2. Add an input abstraction layer for keyboard, mouse, and controller support.
3. Add save-data and settings services under `%LocalAppData%`.
4. Separate platform services behind interfaces before integrating Steamworks or Xbox services.
5. Define release configs for Steam depots and Microsoft Store / Game Pass packaging later.

## Steam and Game Pass readiness notes
- Keep platform APIs isolated behind adapters.
- Prefer controller-first UX from the start.
- Centralize achievements, cloud saves, entitlement checks, and multiplayer hooks behind service interfaces.
- Avoid hard-coding file paths or overlay-specific behavior.
- Plan for store assets, age ratings, sandbox testing, and certification requirements early.
