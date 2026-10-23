using System.Text.Json.Serialization;

namespace DevelopersJourney.Models;

public class DailyQuest : QuestBase
{
    public int Progress { get; set; } // progress "how many pushes to github today"
    public int Goal { get; set; } // goal "out of this many pushes to github"

    [JsonConverter(typeof(JsonStringEnumConverter))] //attribute for Source property below it. convert enum from numbers (by default) to strings so save points remain consistent
    public ProgressSource Source { get; set; } = ProgressSource.Manual; // source of progress for dailies starts as Manual. This writes as a string in the save file
}