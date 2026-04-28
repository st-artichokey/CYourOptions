namespace CYourOptions.Library.Models;

/// <summary>
/// Serializable snapshot of a decision engine's state at a point in time.
/// Used for save/load functionality.
/// </summary>
public class SaveData
{
    /// <summary>
    /// The node the user was on when they saved.
    /// </summary>
    public required string CurrentNodeId { get; init; }

    /// <summary>
    /// Ordered list of node IDs visited before the current node.
    /// </summary>
    public required List<string> History { get; init; }

    /// <summary>
    /// When this save was created. Defaults to now if not specified.
    /// </summary>
    public DateTime SavedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Optional user-facing name for this save slot (e.g., "Autosave", "Before the dragon").
    /// </summary>
    public string? Label { get; init; }
}
