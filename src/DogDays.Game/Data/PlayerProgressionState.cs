namespace DogDays.Game.Data;

/// <summary>
/// Session-scoped player progression flags that can unlock gameplay capabilities.
/// </summary>
public sealed class PlayerProgressionState
{
    /// <summary>
    /// Whether the current forest starter weapon loadout has been intentionally unlocked.
    /// </summary>
    public bool HasForestStarterWeapons { get; set; }

    /// <summary>
    /// Resets all progression unlocks back to their default new-game state.
    /// </summary>
    public void Reset()
    {
        HasForestStarterWeapons = false;
    }
}