using System.Text.Json.Serialization;

namespace DevelopersJourney.Models;

public class WakaTimeTotalSeconds
{
    [JsonPropertyName("total_seconds")]
    public float TotalSeconds { get; set; }
}