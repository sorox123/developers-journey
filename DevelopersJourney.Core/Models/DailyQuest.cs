namespace DevelopersJourney.Models;

public class DailyQuest : Quest
{
    public int Progress { get; set; } // progress "how many pushes to github today"
    public int Goal { get; set; } // goal "out of this many pushes to github"
}