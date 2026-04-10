namespace DogDays.Game.Data.Save;

/// <summary>
/// Snapshot of player progression unlock flags for save/load.
/// </summary>
internal sealed class SavePlayerProgressionData
{
    /// <summary>
    /// Whether the forest starter weapon loadout has been unlocked.
    /// </summary>
    public bool HasForestStarterWeapons { get; set; }
}