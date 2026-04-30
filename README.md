# CYourOptions

A branching decision-tree game engine built with C# and Godot 4.

Stories are authored as CSV files and loaded at runtime — you can add or edit story content without recompiling the game.

## Project Structure

```
src/
  CYourOptions.Library/   Core engine (DecisionEngine, SaveManager, CsvStoryLoader)
  CYourOptions.Game/      Godot 4 frontend
  CYourOptions.Tests/     xUnit test suite
tools/
  CsvStoryGenerator/      CLI tool to generate C# classes from CSV (for non-Godot contexts)
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Godot Mono 4.6+](https://godotengine.org/download) (the .NET version — the standard build cannot load C# scripts). On macOS: `brew install godot-mono`

## Quick Start

```bash
# Build the C# project
dotnet build src/CYourOptions.Game

# Run the game (use godot-mono if installed via Homebrew)
godot --path src/CYourOptions.Game

# Run tests
dotnet test src/CYourOptions.Tests
```

You can also open the project in the Godot editor: Launch Godot → Import → select `src/CYourOptions.Game/project.godot`.

## Adding a Story

1. Create a folder under `src/CYourOptions.Game/stories/<your-story-name>/`
2. Add `nodes.csv` with columns: `Id,Title,Text`
3. Add `choices.csv` with columns: `FromNodeId,Label,NextNodeId`
4. In the Godot editor, set the GameController's `StoryPath` export to `res://stories/<your-story-name>`

The first node in `nodes.csv` is the start node. Nodes with no outgoing choices are endings.

See `tools/CsvStoryGenerator/examples/` for the expected CSV format.

## CSV Format

**nodes.csv**
```csv
Id,Title,Text
start,Opening,"You wake up and see two doors."
left_door,Left Door,"You open the left door..."
right_door,Right Door,"You open the right door..."
```

**choices.csv**
```csv
FromNodeId,Label,NextNodeId
start,Open the left door,left_door
start,Open the right door,right_door
```

## Architecture

The Library is platform-agnostic — it parses CSV strings and provides tree traversal. It has no file I/O or UI dependencies. The Godot frontend handles asset loading and rendering.

```
CSV files → CsvStoryLoader.ParseFromCsv() → List<DecisionNode> → DecisionEngine → UI
```

## Running the Code Generator

The `CsvStoryGenerator` tool converts CSV stories into standalone C# classes (useful for contexts where runtime CSV loading isn't available):

```bash
dotnet run --project tools/CsvStoryGenerator -- \
  --nodes path/to/nodes.csv \
  --choices path/to/choices.csv \
  --output path/to/Output.cs \
  --class-name MyStory
```
