namespace DevelopersJourney.Models;

public enum ProgressSource
{
    Manual, //player clicks +1
    GitHubPushes, //fed by GitHub API
    CodingHours //fed by WakaTime for now, custom tracker later
}