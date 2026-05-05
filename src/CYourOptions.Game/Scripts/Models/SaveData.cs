namespace CYourOptions.Game.Scripts.Models;

public class SaveData
{
    public required string CurrentNodeId { get; init; }

    public required List<string> History { get; init; }

    public DateTime SavedAt { get; init; } = DateTime.UtcNow;
}
