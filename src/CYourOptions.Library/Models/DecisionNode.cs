namespace CYourOptions.Library.Models;

/// <summary>
/// A single node in a decision tree, containing text and zero or more choices.
/// Nodes with no choices are leaf/end nodes.
/// </summary>
public class DecisionNode
{
    /// <summary>
    /// Unique identifier for this node, used for navigation and save/load.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Short heading displayed for this node.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Full narrative or prompt text shown to the user.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Available options the user can select. Empty list indicates an end node.
    /// </summary>
    public List<Choice> Choices { get; init; } = [];

    /// <summary>
    /// True when this node has no choices, indicating a terminal state.
    /// </summary>
    public bool IsEndNode => Choices.Count == 0;
}
