using System.Text.Json.Serialization;

namespace DevelopersJourney.Models;

// parses the name field of the json response
public class GitHubRepo
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}