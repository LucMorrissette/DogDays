#nullable enable

using Microsoft.Xna.Framework;

namespace DogDays.Game.Data;

/// <summary>
/// Declares a scripted world position relative to a named spawn-point anchor.
/// </summary>
/// <param name="SpawnPointId">Optional named spawn point used as the anchor.</param>
/// <param name="Offset">Offset from the resolved anchor in top-left world pixels.</param>
internal readonly record struct ScriptedPositionDirective(string? SpawnPointId, Vector2 Offset);