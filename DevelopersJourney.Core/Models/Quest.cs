using System.Text.Json.Serialization;

namespace DevelopersJourney.Models;

public class Quest : QuestBase
{
    public string Description { get; set; }
    public bool IsMainQuest { get; set; }

    //converts Difficulty enums to string, sets nulled values to Beginner
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Difficulty Difficulty { get; set; } = Difficulty.Beginner;
}