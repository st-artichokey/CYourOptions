using CYourOptions.Library.Models;
using CYourOptions.Library.Services;

namespace CYourOptions.Library.Tests;

public class SaveManagerTests
{
    private static List<DecisionNode> CreateSampleTree() =>
    [
        new DecisionNode
        {
            Id = "start",
            Title = "Start",
            Text = "Begin here.",
            Choices =
            [
                new Choice { Label = "Next", NextNodeId = "middle" }
            ]
        },
        new DecisionNode
        {
            Id = "middle",
            Title = "Middle",
            Text = "Keep going.",
            Choices =
            [
                new Choice { Label = "Finish", NextNodeId = "end" }
            ]
        },
        new DecisionNode
        {
            Id = "end",
            Title = "End",
            Text = "Done."
        }
    ];

    [Fact]
    public void CreateSnapshot_CapturesCurrentState()
    {
        var engine = new DecisionEngine(CreateSampleTree());
        engine.Start("start");
        engine.MakeChoice(0); // middle
        var manager = new SaveManager();

        var save = manager.CreateSnapshot(engine);

        Assert.Equal("middle", save.CurrentNodeId);
        Assert.Equal(["start"], save.History);
    }

    [Fact]
    public void CreateSnapshot_BeforeStart_Throws()
    {
        var engine = new DecisionEngine(CreateSampleTree());
        var manager = new SaveManager();

        Assert.Throws<InvalidOperationException>(() => manager.CreateSnapshot(engine));
    }

    [Fact]
    public void Serialize_ProducesValidJson()
    {
        var manager = new SaveManager();
        var save = new SaveData
        {
            CurrentNodeId = "middle",
            History = ["start"],
            SavedAt = new DateTime(2026, 4, 28, 12, 0, 0, DateTimeKind.Utc)
        };

        var json = manager.Serialize(save);

        Assert.Contains("\"currentNodeId\": \"middle\"", json);
    }

    [Fact]
    public void Deserialize_RoundTrips()
    {
        var manager = new SaveManager();
        var original = new SaveData
        {
            CurrentNodeId = "end",
            History = ["start", "middle"],
            SavedAt = new DateTime(2026, 4, 28, 12, 0, 0, DateTimeKind.Utc)
        };

        var json = manager.Serialize(original);
        var restored = manager.Deserialize(json);

        Assert.Equal(original.CurrentNodeId, restored.CurrentNodeId);
        Assert.Equal(original.History, restored.History);
    }

    [Fact]
    public void RestoreSnapshot_RebuildsEngineState()
    {
        var tree = CreateSampleTree();
        var engine = new DecisionEngine(tree);
        var manager = new SaveManager();

        var save = new SaveData
        {
            CurrentNodeId = "middle",
            History = ["start"]
        };

        manager.RestoreSnapshot(engine, save);

        Assert.Equal("middle", engine.CurrentNode!.Id);
        Assert.Equal(["start"], engine.History);
    }

    [Fact]
    public void RestoreSnapshot_AtStartNode_Works()
    {
        var tree = CreateSampleTree();
        var engine = new DecisionEngine(tree);
        var manager = new SaveManager();

        var save = new SaveData
        {
            CurrentNodeId = "start",
            History = []
        };

        manager.RestoreSnapshot(engine, save);

        Assert.Equal("start", engine.CurrentNode!.Id);
        Assert.Empty(engine.History);
    }

    [Fact]
    public void FullSaveLoadCycle()
    {
        var tree = CreateSampleTree();
        var engine = new DecisionEngine(tree);
        engine.Start("start");
        engine.MakeChoice(0); // middle
        var manager = new SaveManager();

        var json = manager.Serialize(manager.CreateSnapshot(engine));

        // New engine, restore from JSON
        var engine2 = new DecisionEngine(tree);
        var loadedSave = manager.Deserialize(json);
        manager.RestoreSnapshot(engine2, loadedSave);

        Assert.Equal(engine.CurrentNode!.Id, engine2.CurrentNode!.Id);
        Assert.Equal(engine.History, engine2.History);
    }
}
