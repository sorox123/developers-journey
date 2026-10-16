using System.Text.Json.Serialization;

namespace DevelopersJourney.Models;

public class WakaTimeData
{
    [JsonPropertyName("data")]
    public List<WakaTimeGrandTotal> Data { get; set; }
}