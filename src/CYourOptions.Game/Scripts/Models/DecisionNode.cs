namespace CYourOptions.Game.Scripts.Models;

public class DecisionNode
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required string Text { get; init; }

    public List<Choice> Choices { get; init; } = [];

    public bool IsEndNode => Choices.Count == 0;
}
