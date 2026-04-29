using Godot;
using CYourOptions.Library.Models;
using CYourOptions.Library.Services;

namespace CYourOptions.Game.Scripts;

public partial class GameController : Control
{
    [Export] public string StoryPath { get; set; } = "res://stories/the-production-incident";

    private DecisionEngine _engine = null!;
    private string _startNodeId = null!;

    private Label _titleLabel = null!;
    private RichTextLabel _textLabel = null!;
    private VBoxContainer _choicesContainer = null!;
    private Button _backButton = null!;
    private Button _restartButton = null!;
    private Label _endLabel = null!;

    public override void _Ready()
    {
        var nodes = LoadStory(StoryPath);
        _engine = new DecisionEngine(nodes);

        _titleLabel = GetNode<Label>("%TitleLabel");
        _textLabel = GetNode<RichTextLabel>("%TextLabel");
        _choicesContainer = GetNode<VBoxContainer>("%ChoicesContainer");
        _backButton = GetNode<Button>("%BackButton");
        _restartButton = GetNode<Button>("%RestartButton");
        _endLabel = GetNode<Label>("%EndLabel");

        _backButton.Pressed += OnBackPressed;
        _restartButton.Pressed += OnRestartPressed;

        _startNodeId = nodes[0].Id;
        _engine.Start(_startNodeId);
        UpdateUI();
    }

    private static List<DecisionNode> LoadStory(string storyPath)
    {
        var nodesPath = $"{storyPath}/nodes.csv";
        var choicesPath = $"{storyPath}/choices.csv";

        var nodesFile = FileAccess.Open(nodesPath, FileAccess.ModeFlags.Read);
        if (nodesFile is null)
            throw new System.IO.FileNotFoundException(
                $"Could not open {nodesPath}: {FileAccess.GetOpenError()}");

        var choicesFile = FileAccess.Open(choicesPath, FileAccess.ModeFlags.Read);
        if (choicesFile is null)
        {
            nodesFile.Dispose();
            throw new System.IO.FileNotFoundException(
                $"Could not open {choicesPath}: {FileAccess.GetOpenError()}");
        }

        using (nodesFile)
        using (choicesFile)
        {
            return CsvStoryLoader.ParseFromCsv(nodesFile.GetAsText(), choicesFile.GetAsText());
        }
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

        for (int i = 0; i < node.Choices.Count; i++)
        {
            var button = new Button();
            button.Text = node.Choices[i].Label;
            button.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            button.AddThemeFontSizeOverride("font_size", 28);
            int choiceIndex = i;
            button.Pressed += () =>
            {
                _engine.MakeChoice(choiceIndex);
                UpdateUI();
            };
            _choicesContainer.AddChild(button);
        }

        _backButton.Visible = _engine.CanGoBack;
        _restartButton.Visible = node.IsEndNode;
        _endLabel.Visible = node.IsEndNode;
        _choicesContainer.Visible = !node.IsEndNode;
    }

    private void OnBackPressed()
    {
        _engine.GoBack();
        UpdateUI();
    }

    private void OnRestartPressed()
    {
        _engine.Start(_startNodeId);
        UpdateUI();
    }
}
