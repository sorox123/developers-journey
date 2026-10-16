using System.Text.Json.Serialization;

namespace DevelopersJourney.Models;

public class WakaTimeProject
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("total_seconds")]
    public float TotalSeconds { get; set; }
}