namespace DogDays.Game.Data;

/// <summary>
/// Declares a scripted actor pose by combining a position directive with a facing direction.
/// </summary>
/// <param name="Position">Position directive used to resolve the actor's top-left world position.</param>
/// <param name="Facing">Facing direction the actor should hold for the pose.</param>
internal readonly record struct ScriptedActorPoseDirective(ScriptedPositionDirective Position, FacingDirection Facing);