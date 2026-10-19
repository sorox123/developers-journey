using System.Text.Json.Serialization;

namespace DevelopersJourney.Models;

public class WakaTimeGrandTotal
{
    [JsonPropertyName("grand_total")]
    public WakaTimeTotalSeconds GrandTotal { get; set; }

    [JsonPropertyName("projects")]
    public List<WakaTimeProject> Projects { get; set; }
}