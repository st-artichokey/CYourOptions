namespace CYourOptions.Game.Scripts.Models;

public class Choice
{
    public required string Label { get; init; }

    public required string NextNodeId { get; init; }

    public string? Description { get; init; }
}
