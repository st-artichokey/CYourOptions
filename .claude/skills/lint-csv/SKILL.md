---
name: lint-csv
description: Validate story CSV files (nodes + choices) for structural errors, narrative issues, and authoring mistakes before running the code generator.
argument-hint: "[path/to/nodes.csv path/to/choices.csv]"
---

# Lint CSV — Story CSV Validator

Validate a pair of story CSV files without compiling, catching issues early in the authoring workflow.

## What to check

### Structural validity
- Both files parse as valid CSV (correct quoting, consistent column counts)
- Required headers present: nodes.csv needs `Id,Title,Text`; choices.csv needs `FromNodeId,Label,NextNodeId`
- No empty Id, Title, Text, FromNodeId, Label, or NextNodeId fields
- No duplicate node IDs
- All `FromNodeId` values in choices.csv exist in nodes.csv
- All `NextNodeId` values in choices.csv exist in nodes.csv
- No orphan nodes (defined in nodes.csv but never referenced by any choice and not the first node)

### Graph integrity
- First node in nodes.csv is treated as start node
- All nodes are reachable from the start node
- Every non-ending node has at least one choice
- No infinite loops without an exit (cycles must have at least one path out)
- Every path eventually reaches an ending (node with no outgoing choices)

### Narrative quality
- Character names are consistent across all node text (flag variations like "Jamie" vs "James")
- No duplicate choice labels within the same node
- Choice labels are distinct and descriptive (flag generic labels like "Option 1", "Choice A")
- Flag placeholder text (TODO, TBD, FIXME, lorem ipsum, [brackets])
- Flag excessively short node text (<20 characters)

### Pacing
- Note longest path length in the summary
- Flag paths shorter than 2 choices to reach an ending
- Flag nodes with only 1 choice (linear segments)
- Report total path count and ending count

## How to execute

1. If `$ARGUMENTS` contains two paths, use those. Otherwise look for `nodes.csv` and `choices.csv` in the current directory or `tools/CsvStoryGenerator/examples/`.
2. Read both CSV files
3. Parse and validate structure (headers, column counts, quoting)
4. Build an adjacency graph from the data
5. Run all checks listed above
6. Attempt to load via the library tests as a final validation: `dotnet test CYourOptions/src/CYourOptions.Tests --filter "FullyQualifiedName~TheProductionIncident"` (if it errors, capture the message)
7. Report findings grouped by severity

## Output format

```
## CSV Lint Report

### Files
- Nodes: [path] ([N] nodes)
- Choices: [path] ([N] choices)

### Summary
- Start node: [id]
- Endings: [N]
- Total paths: [N]
- Issues found: [N] errors, [N] warnings, [N] suggestions

### Errors (must fix before generating)
(list or "None")

### Warnings
(list or "None")

### Suggestions
(list or "None")
```

Report facts only. Do not editorialize, suggest fixes, explain why something matters, or add commentary beyond identifying the issue and its location.

## Arguments

`$ARGUMENTS` should be two space-separated paths: the nodes CSV followed by the choices CSV. If omitted, searches for `nodes.csv` and `choices.csv` in common locations.
