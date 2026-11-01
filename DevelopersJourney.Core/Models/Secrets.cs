namespace DevelopersJourney.Models;

//stores tokens here (defined in gitignore)
public class Secrets //all properties are initialized as empty strings, making them optional
{
    public string GitHubToken { get; set; } = "";
    public string WakaTimeToken { get; set; } = "";
    public string GitHubUsername { get; set; } = ""; //sets GitHub username as empty string placeholder
}