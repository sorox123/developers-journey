using DevelopersJourney.Models;
using System.Text.Json;

namespace DevelopersJourney.Services;

public class SaveService
{
    public const int CurrentVersion = 1;
    public SaveData LoadGame()
    {
        if (File.Exists("quests.json"))
        {
            try
            {
                string json = File.ReadAllText("quests.json");
                SaveData save = JsonSerializer.Deserialize<SaveData>(json);
                return UpgradeSave(save);
            }
            catch (Exception ex)
            {
                return LoadFromDefinitions();
                
            }
        }
        else
        {
            return LoadFromDefinitions();
        }
    }

    public void SaveGame(List<Quest> quests, int totalXP, List<DailyQuest> dailyQuests, DateTime lastDailyReset)
    {
        SaveData saveOut = new SaveData();
        saveOut.Version = CurrentVersion;
        saveOut.LastDailyReset = lastDailyReset;
        saveOut.DailyQuests = dailyQuests;
        saveOut.Quests = quests;
        saveOut.TotalXP = totalXP;

        string data = JsonSerializer.Serialize(saveOut);
        File.WriteAllText("quests.json", data);
    }

    public SaveData LoadFromDefinitions()
    {
        try
        {
            string questDefinitions = File.ReadAllText("questdefinitions.json");
            List<Quest> quests = JsonSerializer.Deserialize<List<Quest>>(questDefinitions);

            SaveData questsList = new SaveData();
            questsList.Quests = quests;
            questsList.TotalXP = 0;
            questsList.DailyQuests = LoadDailyQuestFromDefinitions();
            questsList.LastDailyReset = DateTime.Today.AddHours(2);
            questsList.Version = CurrentVersion;
            return questsList;
        }
        catch
        {
            throw new FileNotFoundException("Fatal error: could not load quest definitions.");
        }
    }

    public List<DailyQuest> LoadDailyQuestFromDefinitions()
    {
        try
        {
            string dailyQuestDefinitions = File.ReadAllText("dailyquestdefinitions.json");
            List<DailyQuest> dailyQuests = JsonSerializer.Deserialize<List<DailyQuest>>(dailyQuestDefinitions);

            return dailyQuests;
        }
        catch
        {
            throw new FileNotFoundException("Fatal error: could not load daily quest definitions");
        }
    }
    private SaveData UpgradeSave(SaveData save)
    {
        if (save.Version == 0)
        {
            if (save.DailyQuests == null)
            {
                save.DailyQuests = LoadDailyQuestFromDefinitions();
            }

            save.Version = 1;
        }

        return save;
    }
}