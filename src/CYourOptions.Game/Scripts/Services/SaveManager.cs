using System.Text.Json;
using CYourOptions.Game.Scripts.Models;

namespace CYourOptions.Game.Scripts.Services;

public class SaveManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SaveData CreateSnapshot(DecisionEngine engine)
    {
        if (engine.CurrentNode is null)
            throw new InvalidOperationException("Cannot save — engine has not been started.");

        return new SaveData
        {
            CurrentNodeId = engine.CurrentNode.Id,
            History = engine.History.ToList(),
            SavedAt = DateTime.UtcNow
        };
    }

    public void RestoreSnapshot(DecisionEngine engine, SaveData data)
    {
        if (data.History.Count == 0)
        {
            engine.Start(data.CurrentNodeId);
            return;
        }

        engine.Start(data.History[0]);
        foreach (var nextNodeId in data.History.Skip(1).Append(data.CurrentNodeId))
        {
            var current = engine.CurrentNode!;
            var choiceIndex = current.Choices.FindIndex(c => c.NextNodeId == nextNodeId);

            if (choiceIndex < 0)
                throw new InvalidOperationException(
                    $"Cannot restore: no choice from '{current.Id}' leads to '{nextNodeId}'.");

            engine.MakeChoice(choiceIndex);
        }
    }

    public string Serialize(SaveData data)
    {
        return JsonSerializer.Serialize(data, JsonOptions);
    }

    public SaveData Deserialize(string json)
    {
        return JsonSerializer.Deserialize<SaveData>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize save data.");
    }
}
