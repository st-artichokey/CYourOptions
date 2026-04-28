using System.Text.Json;
using CYourOptions.Library.Models;

namespace CYourOptions.Library.Services;

/// <summary>
/// Handles serialization and state transfer for decision engine save/load.
/// Does not perform file I/O — the caller provides strings in and out.
/// </summary>
public class SaveManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Capture the engine's current state as a serializable object.
    /// </summary>
    /// <exception cref="InvalidOperationException">Engine has not been started.</exception>
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

    /// <summary>
    /// Replay saved history onto the engine, restoring it to the saved position.
    /// Walks the tree from the first history node forward, validating each step.
    /// </summary>
    /// <exception cref="InvalidOperationException">Save data contains an invalid path through the tree.</exception>
    public void RestoreSnapshot(DecisionEngine engine, SaveData data)
    {
        engine.Start(data.History.Count > 0 ? data.History[0] : data.CurrentNodeId);

        for (int i = 1; i < data.History.Count; i++)
        {
            var current = engine.CurrentNode!;
            var nextNodeId = data.History[i];
            var choiceIndex = current.Choices.FindIndex(c => c.NextNodeId == nextNodeId);

            if (choiceIndex < 0)
                throw new InvalidOperationException(
                    $"Cannot restore: no choice from '{current.Id}' leads to '{nextNodeId}'.");

            engine.MakeChoice(choiceIndex);
        }

        if (data.History.Count > 0)
        {
            var current = engine.CurrentNode!;
            var choiceIndex = current.Choices.FindIndex(c => c.NextNodeId == data.CurrentNodeId);

            if (choiceIndex < 0)
                throw new InvalidOperationException(
                    $"Cannot restore: no choice from '{current.Id}' leads to '{data.CurrentNodeId}'.");

            engine.MakeChoice(choiceIndex);
        }
    }

    /// <summary>
    /// Convert save data to a JSON string for storage.
    /// </summary>
    public string Serialize(SaveData data)
    {
        return JsonSerializer.Serialize(data, JsonOptions);
    }

    /// <summary>
    /// Parse a JSON string back into save data.
    /// </summary>
    /// <exception cref="InvalidOperationException">JSON is null or malformed.</exception>
    public SaveData Deserialize(string json)
    {
        return JsonSerializer.Deserialize<SaveData>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize save data.");
    }
}
