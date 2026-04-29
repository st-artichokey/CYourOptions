using System.Text;
using CYourOptions.Library.Models;
using CYourOptions.Library.Services;

if (args.Length == 0 || args.Contains("--help"))
{
    Console.WriteLine("Usage: CsvStoryGenerator --nodes <path> --choices <path> --output <path> --class-name <name>");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --nodes       Path to the nodes CSV file (Id, Title, Text)");
    Console.WriteLine("  --choices     Path to the choices CSV file (FromNodeId, Label, NextNodeId, Description)");
    Console.WriteLine("  --output      Path for the generated .cs file");
    Console.WriteLine("  --class-name  Name of the generated static class");
    Console.WriteLine("  --namespace   Namespace for the generated class (default: CYourOptions.Library.Stories)");
    return 0;
}

string? nodesPath = null, choicesPath = null, outputPath = null, className = null;
string namespaceName = "CYourOptions.Library.Stories";

for (int i = 0; i < args.Length - 1; i++)
{
    switch (args[i])
    {
        case "--nodes": nodesPath = args[++i]; break;
        case "--choices": choicesPath = args[++i]; break;
        case "--output": outputPath = args[++i]; break;
        case "--class-name": className = args[++i]; break;
        case "--namespace": namespaceName = args[++i]; break;
    }
}

if (nodesPath is null || choicesPath is null || outputPath is null || className is null)
{
    Console.Error.WriteLine("Error: --nodes, --choices, --output, and --class-name are all required.");
    return 1;
}

if (!File.Exists(nodesPath))
{
    Console.Error.WriteLine($"Error: Nodes file not found: {nodesPath}");
    return 1;
}

if (!File.Exists(choicesPath))
{
    Console.Error.WriteLine($"Error: Choices file not found: {choicesPath}");
    return 1;
}

List<DecisionNode> nodes;
try
{
    nodes = CsvStoryLoader.LoadFromCsv(nodesPath, choicesPath);
}
catch (FormatException ex)
{
    Console.Error.WriteLine($"Validation error: {ex.Message}");
    return 1;
}

var startNodeId = nodes[0].Id;
var sb = new StringBuilder();
sb.AppendLine("using CYourOptions.Library.Models;");
sb.AppendLine();
sb.AppendLine($"namespace {namespaceName};");
sb.AppendLine();
sb.AppendLine($"public static class {className}");
sb.AppendLine("{");
sb.AppendLine($"    public const string StartNodeId = \"{Escape(startNodeId)}\";");
sb.AppendLine();
sb.AppendLine("    public static List<DecisionNode> GetNodes() =>");
sb.AppendLine("    [");

for (int i = 0; i < nodes.Count; i++)
{
    var node = nodes[i];
    sb.AppendLine("        new DecisionNode");
    sb.AppendLine("        {");
    sb.AppendLine($"            Id = \"{Escape(node.Id)}\",");
    sb.AppendLine($"            Title = \"{Escape(node.Title)}\",");
    sb.AppendLine($"            Text = \"{Escape(node.Text)}\"" + (node.Choices.Count > 0 ? "," : ""));

    if (node.Choices.Count > 0)
    {
        sb.AppendLine("            Choices =");
        sb.AppendLine("            [");
        for (int j = 0; j < node.Choices.Count; j++)
        {
            var choice = node.Choices[j];
            var desc = choice.Description is not null
                ? $", Description = \"{Escape(choice.Description)}\""
                : "";
            var comma = j < node.Choices.Count - 1 ? "," : "";
            sb.AppendLine($"                new Choice {{ Label = \"{Escape(choice.Label)}\", NextNodeId = \"{Escape(choice.NextNodeId)}\"{desc} }}{comma}");
        }
        sb.AppendLine("            ]");
    }

    sb.AppendLine("        }" + (i < nodes.Count - 1 ? "," : ""));
    sb.AppendLine();
}

sb.AppendLine("    ];");
sb.AppendLine("}");

File.WriteAllText(outputPath, sb.ToString());
Console.WriteLine($"Generated {outputPath} with {nodes.Count} nodes ({nodes.Count(n => n.IsEndNode)} endings).");
return 0;

static string Escape(string s) => s
    .Replace("\\", "\\\\")
    .Replace("\"", "\\\"")
    .Replace("\n", "\\n")
    .Replace("\r", "\\r");
