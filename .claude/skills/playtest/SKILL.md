---
name: playtest
description: Check story paths for logical issues, dead ends, unreachable nodes, and narrative inconsistencies. Use when validating story content or after adding/modifying a decision tree.
argument-hint: "[story-class-name or 'all']"
---

# Playtest — Story Path Validator

Systematically walk every path through a decision tree story, checking for issues.

## What to check

### Structural integrity
- All node IDs referenced in choices actually exist
- All nodes are reachable from the start node
- No orphan nodes (defined but unreachable)
- Every non-ending path eventually reaches an ending
- No infinite loops (cycles without an exit)

### Narrative consistency
- Character names used consistently (no "Jamie" in one node and "James" in another)
- Tense consistency within each node's text
- No references to events that only happen on other branches (e.g., "after you reverted" when the player never chose to revert)
- Choices make sense given the context of the current node
- No duplicate choice labels within the same node

### Pacing and balance
- Flag paths that are excessively long (>8 nodes to reach an ending)
- Flag paths that are too short (ending reached in <2 choices)
- Identify nodes with only 1 choice (linear segments — are they necessary?)
- Check that endings feel earned (not abrupt dead ends)

### Quality
- Flag any placeholder or TODO text
- Check for typos or grammatical issues in node titles and text
- Ensure choice labels are distinct and meaningful (not "Option A" / "Option B")

## How to execute

1. Read the story class file from `src/CYourOptions.Library/Stories/`
2. Parse all nodes and build an adjacency graph
3. Run the existing xUnit tests to confirm structural basics pass: `dotnet test src/CYourOptions.Tests --filter "FullyQualifiedName~$ARGUMENTS"`
4. Walk every unique path from start to each ending, documenting the route
5. Check each item in the lists above
6. Report findings grouped by severity:
   - **Blocking** — broken paths, missing nodes, crashes
   - **Warning** — narrative inconsistencies, pacing concerns
   - **Suggestion** — quality improvements, optional polish

## Output format

```
## Playtest Report: [Story Name]

### Summary
- Paths tested: X
- Endings reached: X/Y
- Issues found: X blocking, X warnings, X suggestions

### Blocking Issues
(list or "None")

### Warnings
(list or "None")

### Suggestions
(list or "None")

### Path Trace
(shortest path to each ending, for quick reference)
```

## Arguments

If `$ARGUMENTS` is provided, scope the playtest to that specific story class. If "all" or empty, test all stories in the Stories folder.
