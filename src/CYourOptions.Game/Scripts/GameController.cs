using Godot;
using CYourOptions.Library.Models;
using CYourOptions.Library.Services;

namespace CYourOptions.Game.Scripts;

public partial class GameController : Control
{
    private const string StartNodeId = "monday_morning";

    private DecisionEngine _engine = null!;
    private SaveManager _saveManager = null!;

    private Label _titleLabel = null!;
    private RichTextLabel _textLabel = null!;
    private VBoxContainer _choicesContainer = null!;
    private Button _backButton = null!;
    private Button _restartButton = null!;
    private Label _endLabel = null!;

    public override void _Ready()
    {
        var nodes = LoadStoryFromResources();
        _engine = new DecisionEngine(nodes);
        _saveManager = new SaveManager();

        _titleLabel = GetNode<Label>("%TitleLabel");
        _textLabel = GetNode<RichTextLabel>("%TextLabel");
        _choicesContainer = GetNode<VBoxContainer>("%ChoicesContainer");
        _backButton = GetNode<Button>("%BackButton");
        _restartButton = GetNode<Button>("%RestartButton");
        _endLabel = GetNode<Label>("%EndLabel");

        _backButton.Pressed += OnBackPressed;
        _restartButton.Pressed += OnRestartPressed;

        _engine.Start(StartNodeId);
        UpdateUI();
    }

    private static List<DecisionNode> LoadStoryFromResources()
    {
        using var nodesFile = FileAccess.Open("res://nodes.csv", FileAccess.ModeFlags.Read);
        using var choicesFile = FileAccess.Open("res://choices.csv", FileAccess.ModeFlags.Read);

        var nodesCsv = nodesFile.GetAsText();
        var choicesCsv = choicesFile.GetAsText();

        return CsvStoryLoader.ParseFromCsv(nodesCsv, choicesCsv);
    }

    private void UpdateUI()
    {
        var node = _engine.CurrentNode!;

        _titleLabel.Text = node.Title;
        _textLabel.Text = node.Text;

        foreach (var child in _choicesContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var choice in node.Choices)
        {
            var button = new Button();
            button.Text = choice.Label;
            button.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            button.AddThemeFontSizeOverride("font_size", 28);
            var nextNodeId = choice.NextNodeId;
            button.Pressed += () => OnChoicePressed(nextNodeId);
            _choicesContainer.AddChild(button);
        }

        _backButton.Visible = _engine.CanGoBack;
        _restartButton.Visible = node.IsEndNode;
        _endLabel.Visible = node.IsEndNode;
        _choicesContainer.Visible = !node.IsEndNode;
    }

    private void OnChoicePressed(string nextNodeId)
    {
        var node = _engine.CurrentNode!;
        var index = node.Choices.FindIndex(c => c.NextNodeId == nextNodeId);
        _engine.MakeChoice(index);
        UpdateUI();
    }

    private void OnBackPressed()
    {
        _engine.GoBack();
        UpdateUI();
    }

    private void OnRestartPressed()
    {
        _engine.Start(StartNodeId);
        UpdateUI();
    }
}
