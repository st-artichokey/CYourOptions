using CYourOptions.Library.Models;

namespace CYourOptions.Library.Services;

/// <summary>
/// Manages traversal through a decision tree. Tracks the current position
/// and full history, enabling forward navigation, undo, and reset.
/// </summary>
public class DecisionEngine
{
    private readonly Dictionary<string, DecisionNode> _nodes;
    private readonly List<string> _history = [];
    private string? _currentNodeId;

    /// <param name="nodes">All nodes in the decision tree. Indexed by ID for O(1) lookup.</param>
    public DecisionEngine(IEnumerable<DecisionNode> nodes)
    {
        _nodes = nodes.ToDictionary(n => n.Id);
    }

    /// <summary>
    /// The node the user is currently viewing. Null if the engine hasn't started.
    /// </summary>
    public DecisionNode? CurrentNode =>
        _currentNodeId is not null && _nodes.TryGetValue(_currentNodeId, out var node)
            ? node
            : null;

    /// <summary>
    /// Ordered list of previously visited node IDs (not including current).
    /// </summary>
    public IReadOnlyList<string> History => _history;

    public bool CanGoBack => _history.Count > 0;

    public bool IsStarted => _currentNodeId is not null;

    /// <summary>
    /// Begin or restart traversal at the specified node. Clears any existing history.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the node ID doesn't exist in the tree.</exception>
    public void Start(string startNodeId)
    {
        if (!_nodes.ContainsKey(startNodeId))
            throw new ArgumentException($"Node '{startNodeId}' not found in the tree.");

        _currentNodeId = startNodeId;
        _history.Clear();
    }

    /// <summary>
    /// Advance to the node pointed to by the choice at the given index.
    /// Pushes the current node onto the history stack.
    /// </summary>
    /// <param name="choiceIndex">Zero-based index into CurrentNode.Choices.</param>
    /// <exception cref="InvalidOperationException">Engine not started, on an end node, or choice leads to unknown node.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Index outside valid range.</exception>
    public void MakeChoice(int choiceIndex)
    {
        if (CurrentNode is null)
            throw new InvalidOperationException("Engine has not been started. Call Start() first.");

        if (CurrentNode.IsEndNode)
            throw new InvalidOperationException("Cannot make a choice on an end node.");

        if (choiceIndex < 0 || choiceIndex >= CurrentNode.Choices.Count)
            throw new ArgumentOutOfRangeException(nameof(choiceIndex),
                $"Choice index must be between 0 and {CurrentNode.Choices.Count - 1}.");

        var choice = CurrentNode.Choices[choiceIndex];

        if (!_nodes.ContainsKey(choice.NextNodeId))
            throw new InvalidOperationException($"Choice leads to unknown node '{choice.NextNodeId}'.");

        _history.Add(_currentNodeId!);
        _currentNodeId = choice.NextNodeId;
    }

    /// <summary>
    /// Revert to the previous node by popping the last entry from history.
    /// </summary>
    /// <exception cref="InvalidOperationException">No history to revert to.</exception>
    public void GoBack()
    {
        if (!CanGoBack)
            throw new InvalidOperationException("No history to go back to.");

        _currentNodeId = _history[^1];
        _history.RemoveAt(_history.Count - 1);
    }

    /// <summary>
    /// Clear all state — returns the engine to its unstarted condition.
    /// </summary>
    public void Reset()
    {
        _currentNodeId = null;
        _history.Clear();
    }
}
