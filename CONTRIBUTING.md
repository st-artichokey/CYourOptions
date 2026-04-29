# Contributing

## Setup

1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Install [Godot 4.4+](https://godotengine.org/download) (.NET version)
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

Drop `nodes.csv` and `choices.csv` into a new folder under `src/CYourOptions.Game/stories/`. Run `/lint-csv` to validate before testing in-game.

## Validating Stories

Two Claude Code skills are available for story validation:

- `/lint-csv` — Structural validation (broken references, orphan nodes, pacing)
- `/playtest` — Narrative validation (consistency, dead ends, path quality)
