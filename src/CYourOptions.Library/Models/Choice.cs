namespace CYourOptions.Library.Models;

/// <summary>
/// A selectable option within a decision node that leads to another node in the tree.
/// </summary>
public class Choice
{
    /// <summary>
    /// Display text shown to the user for this option.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// The ID of the node this choice leads to.
    /// </summary>
    public required string NextNodeId { get; init; }

    /// <summary>
    /// Optional longer explanation of what this choice means.
    /// </summary>
    public string? Description { get; init; }
}
