using System;
using Microsoft.Xna.Framework;

namespace RiverRats.Game.Input;

/// <summary>
/// Abstraction for querying frame-to-frame input state by logical action.
/// </summary>
public interface IInputManager
{
    /// <summary>Advances input state to the current frame.</summary>
    void Update();

    /// <summary>Returns true while at least one key bound to the action is held.</summary>
    bool IsHeld(InputAction action);

    /// <summary>Returns true only on the frame a bound key transitions up -> down.</summary>
    bool IsPressed(InputAction action);

    /// <summary>Returns true only on the frame a bound key transitions down -> up.</summary>
    bool IsReleased(InputAction action);

    /// <summary>
    /// Returns true only on the frame the left mouse button transitions Released -> Pressed.
    /// </summary>
    /// <remarks>
    /// Unreliable for fast (quick tap) clicks on macOS because the press+release can
    /// both occur between two consecutive <c>Mouse.GetState()</c> polls, causing the
    /// Pressed state to be missed entirely. Prefer <see cref="IsMouseLeftReleased"/> for
    /// click detection.
    /// </remarks>
    [Obsolete("Unreliable for fast clicks on macOS. Use IsMouseLeftReleased() instead.")]
    bool IsMouseLeftPressed();

    /// <summary>
    /// Returns true only on the frame the left mouse button transitions Pressed -> Released.
    /// More reliable than <see cref="IsMouseLeftPressed"/> for detecting fast clicks on macOS,
    /// because the release transition is captured after the button is held for at least one frame.
    /// </summary>
    bool IsMouseLeftReleased();

    /// <summary>Gets the current mouse cursor position in physical window client coordinates.</summary>
    Point GetMousePosition();
}
