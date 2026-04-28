using CYourOptions.Library.Models;
using CYourOptions.Library.Services;

namespace CYourOptions.Library.Tests;

public class DecisionEngineTests
{
    private static List<DecisionNode> CreateSampleTree() =>
    [
        new DecisionNode
        {
            Id = "start",
            Title = "The Beginning",
            Text = "You stand at a crossroads.",
            Choices =
            [
                new Choice { Label = "Go left", NextNodeId = "left" },
                new Choice { Label = "Go right", NextNodeId = "right" }
            ]
        },
        new DecisionNode
        {
            Id = "left",
            Title = "The Left Path",
            Text = "You find a treasure chest.",
            Choices =
            [
                new Choice { Label = "Open it", NextNodeId = "ending_good" },
                new Choice { Label = "Leave it", NextNodeId = "ending_neutral" }
            ]
        },
        new DecisionNode
        {
            Id = "right",
            Title = "The Right Path",
            Text = "A dragon blocks your way.",
            Choices =
            [
                new Choice { Label = "Fight", NextNodeId = "ending_bad" },
                new Choice { Label = "Run", NextNodeId = "start" }
            ]
        },
        new DecisionNode
        {
            Id = "ending_good",
            Title = "Victory",
            Text = "You found gold!"
        },
        new DecisionNode
        {
            Id = "ending_neutral",
            Title = "Nothing Happened",
            Text = "You walked away."
        },
        new DecisionNode
        {
            Id = "ending_bad",
            Title = "Defeat",
            Text = "The dragon wins."
        }
    ];

    [Fact]
    public void Start_SetsCurrentNode()
    {
        var engine = new DecisionEngine(CreateSampleTree());
        engine.Start("start");

        Assert.Equal("start", engine.CurrentNode!.Id);
        Assert.True(engine.IsStarted);
    }

    [Fact]
    public void Start_WithInvalidId_Throws()
    {
        var engine = new DecisionEngine(CreateSampleTree());

        Assert.Throws<ArgumentException>(() => engine.Start("nonexistent"));
    }

    [Fact]
    public void MakeChoice_AdvancesToNextNode()
    {
        var engine = new DecisionEngine(CreateSampleTree());
        engine.Start("start");

        engine.MakeChoice(0); // Go left

        Assert.Equal("left", engine.CurrentNode!.Id);
    }

    [Fact]
    public void MakeChoice_AddsToHistory()
    {
        var engine = new DecisionEngine(CreateSampleTree());
        engine.Start("start");

        engine.MakeChoice(0);

        Assert.Single(engine.History);
        Assert.Equal("start", engine.History[0]);
    }

    [Fact]
    public void MakeChoice_BeforeStart_Throws()
    {
        var engine = new DecisionEngine(CreateSampleTree());

        Assert.Throws<InvalidOperationException>(() => engine.MakeChoice(0));
    }

    [Fact]
    public void MakeChoice_OnEndNode_Throws()
    {
        var engine = new DecisionEngine(CreateSampleTree());
        engine.Start("start");
        engine.MakeChoice(0); // left
        engine.MakeChoice(0); // ending_good

        Assert.True(engine.CurrentNode!.IsEndNode);
        Assert.Throws<InvalidOperationException>(() => engine.MakeChoice(0));
    }

    [Fact]
    public void MakeChoice_InvalidIndex_Throws()
    {
        var engine = new DecisionEngine(CreateSampleTree());
        engine.Start("start");

        Assert.Throws<ArgumentOutOfRangeException>(() => engine.MakeChoice(5));
        Assert.Throws<ArgumentOutOfRangeException>(() => engine.MakeChoice(-1));
    }

    [Fact]
    public void GoBack_ReturnsToPreviousNode()
    {
        var engine = new DecisionEngine(CreateSampleTree());
        engine.Start("start");
        engine.MakeChoice(0); // left

        engine.GoBack();

        Assert.Equal("start", engine.CurrentNode!.Id);
        Assert.Empty(engine.History);
    }

    [Fact]
    public void GoBack_WithNoHistory_Throws()
    {
        var engine = new DecisionEngine(CreateSampleTree());
        engine.Start("start");

        Assert.False(engine.CanGoBack);
        Assert.Throws<InvalidOperationException>(() => engine.GoBack());
    }

    [Fact]
    public void Reset_ClearsAllState()
    {
        var engine = new DecisionEngine(CreateSampleTree());
        engine.Start("start");
        engine.MakeChoice(0);
        engine.MakeChoice(0);

        engine.Reset();

        Assert.Null(engine.CurrentNode);
        Assert.False(engine.IsStarted);
        Assert.Empty(engine.History);
    }

    [Fact]
    public void IsEndNode_TrueWhenNoChoices()
    {
        var engine = new DecisionEngine(CreateSampleTree());
        engine.Start("start");
        engine.MakeChoice(0); // left
        engine.MakeChoice(0); // ending_good

        Assert.True(engine.CurrentNode!.IsEndNode);
    }

    [Fact]
    public void MultipleChoices_TracksFullHistory()
    {
        var engine = new DecisionEngine(CreateSampleTree());
        engine.Start("start");
        engine.MakeChoice(1); // right
        engine.MakeChoice(1); // run back to start
        engine.MakeChoice(0); // left

        Assert.Equal("left", engine.CurrentNode!.Id);
        Assert.Equal(["start", "right", "start"], engine.History);
    }
}
