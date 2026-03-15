using System.Collections.Generic;
using RiverRats.Game.Input;

namespace RiverRats.Tests.Helpers;

/// <summary>
/// Scriptable input fake for integration tests.
/// </summary>
public sealed class FakeInputManager : IInputManager
{
    private readonly HashSet<InputAction> _held = new();
    private readonly HashSet<InputAction> _pressed = new();
    private readonly HashSet<InputAction> _released = new();

    /// <summary>Clears one-frame pressed/released flags and keeps held states.</summary>
    public void Update()
    {
        _pressed.Clear();
        _released.Clear();
    }

    /// <summary>Sets an action as pressed for this frame and held until released.</summary>
    public void Press(InputAction action)
    {
        _pressed.Add(action);
        _held.Add(action);
        _released.Remove(action);
    }

    /// <summary>Sets an action as released for this frame and not held afterward.</summary>
    public void Release(InputAction action)
    {
        _released.Add(action);
        _held.Remove(action);
        _pressed.Remove(action);
    }

    /// <inheritdoc />
    public bool IsHeld(InputAction action)
    {
        return _held.Contains(action);
    }

    /// <inheritdoc />
    public bool IsPressed(InputAction action)
    {
        return _pressed.Contains(action);
    }

    /// <inheritdoc />
    public bool IsReleased(InputAction action)
    {
        return _released.Contains(action);
    }
}
