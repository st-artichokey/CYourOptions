# Contributing

## Setup

1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Install [Godot Mono 4.6+](https://godotengine.org/download) (the .NET version — the standard build cannot load C# scripts). On macOS: `brew install godot-mono`
3. Clone the repo and run `dotnet test src/CYourOptions.Tests` to verify

## Development Workflow

1. Create a branch from `main`
2. Make changes
3. Run tests: `dotnet test src/CYourOptions.Tests`
4. Open a PR

## Project Layout

- `src/CYourOptions.Library/` — Core logic. No Godot or platform dependencies.
- `src/CYourOptions.Game/` — Godot frontend. Open with Godot editor.
- `src/CYourOptions.Tests/` — xUnit tests for the Library.
- `tools/CsvStoryGenerator/` — CLI code generation tool.

## Conventions

- All projects target `net8.0`
- Tests use xUnit
- Story content is CSV — see `tools/CsvStoryGenerator/examples/` for format reference
- Commits follow conventional commits (`feat:`, `fix:`, `chore:`, `refactor:`, `docs:`)

## Adding a Story

Create a folder with `nodes.csv` and `choices.csv`, place it under the Godot project's resource path, and set the GameController's `StoryPath` export to point to it.

