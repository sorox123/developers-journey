namespace DevelopersJourney.Models;

public class SaveData
{
    public List<Quest> Quests { get; set; }
    public List<DailyQuest> DailyQuests { get; set; }
    public int TotalXP { get; set; }
    public DateTime LastDailyReset { get; set; }
    public int Version { get; set; }
}