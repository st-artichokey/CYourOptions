using CYourOptions.Library.Models;
using CYourOptions.Library.Services;
using CYourOptions.Library.Stories;

namespace CYourOptions.Library.Tests;

public class TheProductionIncidentTests
{
    private readonly List<DecisionNode> _nodes = TheProductionIncident.GetNodes();

    [Fact]
    public void AllNodes_HaveUniqueIds()
    {
        var ids = _nodes.Select(n => n.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void AllChoices_PointToExistingNodes()
    {
        var nodeIds = _nodes.Select(n => n.Id).ToHashSet();

        foreach (var node in _nodes)
        {
            foreach (var choice in node.Choices)
            {
                Assert.True(nodeIds.Contains(choice.NextNodeId),
                    $"Node '{node.Id}' has choice '{choice.Label}' pointing to nonexistent node '{choice.NextNodeId}'");
            }
        }
    }

    [Fact]
    public void StartNode_Exists()
    {
        Assert.Contains(_nodes, n => n.Id == TheProductionIncident.StartNodeId);
    }

    [Fact]
    public void StartNode_HasChoices()
    {
        var startNode = _nodes.First(n => n.Id == TheProductionIncident.StartNodeId);
        Assert.NotEmpty(startNode.Choices);
    }

    [Fact]
    public void HasAtLeast20Nodes()
    {
        Assert.True(_nodes.Count >= 20, $"Expected 20+ nodes, got {_nodes.Count}");
    }

    [Fact]
    public void HasMultipleEndings()
    {
        var endings = _nodes.Where(n => n.IsEndNode).ToList();
        Assert.True(endings.Count >= 3, $"Expected 3+ endings, got {endings.Count}");
    }

    [Fact]
    public void AllNodes_AreReachableFromStart()
    {
        var reachable = new HashSet<string>();
        var queue = new Queue<string>();
        queue.Enqueue(TheProductionIncident.StartNodeId);

        while (queue.Count > 0)
        {
            var nodeId = queue.Dequeue();
            if (!reachable.Add(nodeId)) continue;

            var node = _nodes.First(n => n.Id == nodeId);
            foreach (var choice in node.Choices)
            {
                queue.Enqueue(choice.NextNodeId);
            }
        }

        var unreachable = _nodes.Where(n => !reachable.Contains(n.Id)).Select(n => n.Id).ToList();
        Assert.True(unreachable.Count == 0,
            $"Unreachable nodes: {string.Join(", ", unreachable)}");
    }

    [Fact]
    public void Engine_CanPlayThrough_ToEnding()
    {
        var engine = new DecisionEngine(_nodes);
        engine.Start(TheProductionIncident.StartNodeId);

        // Always pick the first choice until we hit an ending
        int steps = 0;
        while (!engine.CurrentNode!.IsEndNode && steps < 50)
        {
            engine.MakeChoice(0);
            steps++;
        }

        Assert.True(engine.CurrentNode.IsEndNode, "Should reach an ending within 50 steps");
    }
}
