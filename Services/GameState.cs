using DevelopersJourney.Models;

namespace DevelopersJourney.Services;

public class GameState
{
    public List<Quest> Quests { get; set; }
    public List<DailyQuest> DailyQuests { get; set; }
    public int TotalXP { get; set; }
    public DateTime LastDailyReset { get; set; }
    public Dictionary<string, ProjectStats> Projects { get; set; }

    public GameState(SaveData saveData)
    {
        Quests = saveData.Quests;
        DailyQuests = saveData.DailyQuests;
        TotalXP = saveData.TotalXP;
        LastDailyReset = saveData.LastDailyReset;
        Projects = new Dictionary<string, ProjectStats>();
    }

    public void CheckForReset()
    {
        DateTime now = DateTime.Now;
        DateTime resetTime = DateTime.Today.AddHours(2);

        if (now < resetTime)
        {
            resetTime = now.AddDays(-1);
        }

        if (LastDailyReset < resetTime)
        {
            for (int i = 0; i < DailyQuests.Count; i++)
            {
                DailyQuests[i].IsComplete = false;
                DailyQuests[i].Progress = 0;
            }
            LastDailyReset = resetTime;
        }
    }
}