using Godot;

namespace CYourOptions.Game.Scripts;

public partial class TitleScreen : Control
{
    private Button _startButton = null!;

    public override void _Ready()
    {
        _startButton = GetNode<Button>("%StartButton");
        _startButton.Pressed += OnStartPressed;
    }

    private void OnStartPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
    }
}
