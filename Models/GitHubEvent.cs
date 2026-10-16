using System.Text.Json.Serialization;

namespace DevelopersJourney.Models;

// parses json response from api
public class GitHubEvent
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("repo")]
    public GitHubRepo Repo { get; set; }
}