using CYourOptions.Library.Models;
using CYourOptions.Library.Services;
using CYourOptions.Library.Stories;

namespace CYourOptions.Tests;

public class CsvStoryLoaderTests
{
    [Fact]
    public void ParseFromCsv_SimpleStory_ReturnsCorrectNodes()
    {
        var nodesCsv = "Id,Title,Text\nstart,Start,\"Welcome to the story\"\nend,End,\"The end\"";
        var choicesCsv = "FromNodeId,Label,NextNodeId,Description\nstart,Go to end,end,";

        var nodes = CsvStoryLoader.ParseFromCsv(nodesCsv, choicesCsv);

        Assert.Equal(2, nodes.Count);
        Assert.Equal("start", nodes[0].Id);
        Assert.Single(nodes[0].Choices);
        Assert.Equal("end", nodes[0].Choices[0].NextNodeId);
        Assert.True(nodes[1].IsEndNode);
    }

    [Fact]
    public void ParseFromCsv_QuotedFieldsWithCommas_ParsesCorrectly()
    {
        var nodesCsv = "Id,Title,Text\nnode1,Title,\"Hello, world. This has \"\"quotes\"\" in it\"";
        var choicesCsv = "FromNodeId,Label,NextNodeId,Description\n";

        var nodes = CsvStoryLoader.ParseFromCsv(nodesCsv, choicesCsv);

        Assert.Single(nodes);
        Assert.Equal("Hello, world. This has \"quotes\" in it", nodes[0].Text);
    }

    [Fact]
    public void ParseFromCsv_ChoiceWithDescription_SetsDescription()
    {
        var nodesCsv = "Id,Title,Text\na,A,TextA\nb,B,TextB";
        var choicesCsv = "FromNodeId,Label,NextNodeId,Description\na,Go,b,Some extra info";

        var nodes = CsvStoryLoader.ParseFromCsv(nodesCsv, choicesCsv);

        Assert.Equal("Some extra info", nodes[0].Choices[0].Description);
    }

    [Fact]
    public void ParseFromCsv_EmptyDescription_SetsNull()
    {
        var nodesCsv = "Id,Title,Text\na,A,TextA\nb,B,TextB";
        var choicesCsv = "FromNodeId,Label,NextNodeId,Description\na,Go,b,";

        var nodes = CsvStoryLoader.ParseFromCsv(nodesCsv, choicesCsv);

        Assert.Null(nodes[0].Choices[0].Description);
    }

    [Fact]
    public void ParseFromCsv_DuplicateNodeId_Throws()
    {
        var nodesCsv = "Id,Title,Text\nnode1,A,TextA\nnode1,B,TextB";
        var choicesCsv = "FromNodeId,Label,NextNodeId,Description\n";

        var ex = Assert.Throws<FormatException>(() => CsvStoryLoader.ParseFromCsv(nodesCsv, choicesCsv));
        Assert.Contains("Duplicate node ID", ex.Message);
    }

    [Fact]
    public void ParseFromCsv_ChoiceReferencesNonExistentTarget_Throws()
    {
        var nodesCsv = "Id,Title,Text\nnode1,A,TextA";
        var choicesCsv = "FromNodeId,Label,NextNodeId,Description\nnode1,Go,missing_node,";

        var ex = Assert.Throws<FormatException>(() => CsvStoryLoader.ParseFromCsv(nodesCsv, choicesCsv));
        Assert.Contains("non-existent node", ex.Message);
    }

    [Fact]
    public void ParseFromCsv_ChoiceFromNonExistentSource_Throws()
    {
        var nodesCsv = "Id,Title,Text\nnode1,A,TextA";
        var choicesCsv = "FromNodeId,Label,NextNodeId,Description\nfake_node,Go,node1,";

        var ex = Assert.Throws<FormatException>(() => CsvStoryLoader.ParseFromCsv(nodesCsv, choicesCsv));
        Assert.Contains("non-existent source node", ex.Message);
    }

    [Fact]
    public void ParseFromCsv_MissingHeader_Throws()
    {
        var nodesCsv = "Id,Title\nnode1,A";
        var choicesCsv = "FromNodeId,Label,NextNodeId,Description\n";

        var ex = Assert.Throws<FormatException>(() => CsvStoryLoader.ParseFromCsv(nodesCsv, choicesCsv));
        Assert.Contains("missing required column", ex.Message);
    }

    [Fact]
    public void RoundTrip_TheProductionIncident_ProducesSameNodeCount()
    {
        var originalNodes = TheProductionIncident.GetNodes();

        var nodesCsv = ExportNodesCsv(originalNodes);
        var choicesCsv = ExportChoicesCsv(originalNodes);

        var reloaded = CsvStoryLoader.ParseFromCsv(nodesCsv, choicesCsv);

        Assert.Equal(originalNodes.Count, reloaded.Count);

        for (int i = 0; i < originalNodes.Count; i++)
        {
            Assert.Equal(originalNodes[i].Id, reloaded[i].Id);
            Assert.Equal(originalNodes[i].Title, reloaded[i].Title);
            Assert.Equal(originalNodes[i].Text, reloaded[i].Text);
            Assert.Equal(originalNodes[i].Choices.Count, reloaded[i].Choices.Count);

            for (int j = 0; j < originalNodes[i].Choices.Count; j++)
            {
                Assert.Equal(originalNodes[i].Choices[j].Label, reloaded[i].Choices[j].Label);
                Assert.Equal(originalNodes[i].Choices[j].NextNodeId, reloaded[i].Choices[j].NextNodeId);
            }
        }
    }

    private static string ExportNodesCsv(List<DecisionNode> nodes)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,Title,Text");
        foreach (var node in nodes)
            sb.AppendLine($"{node.Id},{CsvQuote(node.Title)},{CsvQuote(node.Text)}");
        return sb.ToString();
    }

    private static string ExportChoicesCsv(List<DecisionNode> nodes)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("FromNodeId,Label,NextNodeId,Description");
        foreach (var node in nodes)
            foreach (var choice in node.Choices)
                sb.AppendLine($"{node.Id},{CsvQuote(choice.Label)},{choice.NextNodeId},{CsvQuote(choice.Description ?? "")}");
        return sb.ToString();
    }

    private static string CsvQuote(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}
