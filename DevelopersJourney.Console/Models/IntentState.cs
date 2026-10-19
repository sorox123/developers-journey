namespace DevelopersJourney.Models;

// reads in the meta data for dynamic quest generation
public class IntentState
{
    public Intent IntentType { get; set; }
    public int CurrentGoal { get; set; }
    public int LastSuccessfulGoal { get; set; }
    public int StepSize { get; set; }
    public List<int> RecentHistory { get; set; } = new List<int>();
    public DateTime LastDailyCheck { get; set; }
    public DateTime LastWeeklyCheck { get; set; }
}