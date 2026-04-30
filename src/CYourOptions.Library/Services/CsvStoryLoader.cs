using CYourOptions.Library.Models;

namespace CYourOptions.Library.Services;

public static class CsvStoryLoader
{
    public static List<DecisionNode> LoadFromCsv(string nodesCsvPath, string choicesCsvPath)
    {
        var nodesText = File.ReadAllText(nodesCsvPath);
        var choicesText = File.ReadAllText(choicesCsvPath);
        return ParseFromCsv(nodesText, choicesText);
    }


    public static List<DecisionNode> ParseFromCsv(string nodesCsv, string choicesCsv)
    {
        var nodeRows = ParseCsv(nodesCsv);
        var choiceRows = ParseCsv(choicesCsv);

        ValidateHeaders(nodeRows, ["Id", "Title", "Text"], "nodes");
        ValidateHeaders(choiceRows, ["FromNodeId", "Label", "NextNodeId"], "choices");

        var nodeDict = new Dictionary<string, (string Title, string Text)>();
        foreach (var row in nodeRows.Skip(1))
        {
            if (row.Count < 3)
                throw new FormatException($"Node row has {row.Count} columns, expected at least 3: {string.Join(",", row)}");

            var id = row[0].Trim();
            if (string.IsNullOrWhiteSpace(id))
                continue;

            if (nodeDict.ContainsKey(id))
                throw new FormatException($"Duplicate node ID: '{id}'");

            nodeDict[id] = (row[1].Trim(), row[2].Trim());
        }

        var choicesByNode = new Dictionary<string, List<Choice>>();
        foreach (var row in choiceRows.Skip(1))
        {
            if (row.Count < 3)
                throw new FormatException($"Choice row has {row.Count} columns, expected at least 3: {string.Join(",", row)}");

            var fromId = row[0].Trim();
            if (string.IsNullOrWhiteSpace(fromId))
                continue;

            var label = row[1].Trim();
            var nextId = row[2].Trim();
            var description = row.Count > 3 ? row[3].Trim() : null;
            if (string.IsNullOrWhiteSpace(description))
                description = null;

            if (!choicesByNode.ContainsKey(fromId))
                choicesByNode[fromId] = [];

            choicesByNode[fromId].Add(new Choice
            {
                Label = label,
                NextNodeId = nextId,
                Description = description
            });
        }

        var nodes = new List<DecisionNode>();
        foreach (var (id, (title, text)) in nodeDict)
        {
            nodes.Add(new DecisionNode
            {
                Id = id,
                Title = title,
                Text = text,
                Choices = choicesByNode.GetValueOrDefault(id) ?? []
            });
        }

        Validate(nodes);
        return nodes;
    }

    private static void Validate(List<DecisionNode> nodes)
    {
        var nodeIds = nodes.Select(n => n.Id).ToHashSet();

        foreach (var node in nodes)
        {
            foreach (var choice in node.Choices)
            {
                if (!nodeIds.Contains(choice.NextNodeId))
                    throw new FormatException(
                        $"Node '{node.Id}' has choice '{choice.Label}' pointing to non-existent node: '{choice.NextNodeId}'");
            }
        }
    }

    private static void ValidateHeaders(List<List<string>> rows, string[] expected, string fileLabel)
    {
        if (rows.Count == 0)
            throw new FormatException($"The {fileLabel} CSV is empty.");

        var headers = rows[0].Select(h => h.Trim()).ToList();
        foreach (var col in expected)
        {
            if (!headers.Contains(col, StringComparer.OrdinalIgnoreCase))
                throw new FormatException($"The {fileLabel} CSV is missing required column: '{col}'. Found: {string.Join(", ", headers)}");
        }
    }

    internal static List<List<string>> ParseCsv(string csv)
    {
        var rows = new List<List<string>>();
        var fields = new List<string>();
        var field = new System.Text.StringBuilder();
        bool inQuotes = false;
        int i = 0;

        while (i < csv.Length)
        {
            char c = csv[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < csv.Length && csv[i + 1] == '"')
                    {
                        field.Append('"');
                        i += 2;
                    }
                    else
                    {
                        inQuotes = false;
                        i++;
                    }
                }
                else
                {
                    field.Append(c);
                    i++;
                }
            }
            else
            {
                if (c == '"' && field.Length == 0)
                {
                    inQuotes = true;
                    i++;
                }
                else if (c == ',')
                {
                    fields.Add(field.ToString());
                    field.Clear();
                    i++;
                }
                else if (c == '\r' || c == '\n')
                {
                    fields.Add(field.ToString());
                    field.Clear();
                    rows.Add(fields);
                    fields = [];

                    if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
                        i += 2;
                    else
                        i++;
                }
                else
                {
                    field.Append(c);
                    i++;
                }
            }
        }

        if (field.Length > 0 || fields.Count > 0)
        {
            fields.Add(field.ToString());
            rows.Add(fields);
        }

        return rows;
    }
}
