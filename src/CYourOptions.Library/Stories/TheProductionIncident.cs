using CYourOptions.Library.Models;
using CYourOptions.Library.Services;

namespace CYourOptions.Library.Stories;

public static class TheProductionIncident
{
    public const string StartNodeId = "monday_morning";

    public static List<DecisionNode> GetNodes() =>
        CsvStoryLoader.LoadEmbedded(
            "CYourOptions.Library.Stories.Data.nodes.csv",
            "CYourOptions.Library.Stories.Data.choices.csv");
}
