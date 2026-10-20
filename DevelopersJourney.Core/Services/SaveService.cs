using DevelopersJourney.Models;
using System.Text.Json;

namespace DevelopersJourney.Services;

public class SaveService
{
    public const int CurrentVersion = 1;

    private string dataFolder; //folder for each method in this class

    //contructor: recieves the folder path once, stors is in the dataFolder field
    public SaveService(string folder)
    {
        dataFolder = folder;
    }
    public SaveData LoadGame()
    {
        string savePath = Path.Combine(dataFolder, "quests.json");
        string brokenPath = Path.Combine(dataFolder, "quests.broken.json");

        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                SaveData save = JsonSerializer.Deserialize<SaveData>(json);
                return UpgradeSave(save);
            }
            catch (Exception ex)
            {
                File.Copy(savePath, brokenPath, overwrite: true);
                throw new Exception($"Save file is broken: {ex.Message}. Copied to quests.broken.json");
            }
        }
        else
        {
            return LoadFromDefinitions();
        }
    }

    public void SaveGame(GameState game)
    {
        SaveData saveOut = new SaveData();
        saveOut.Version = CurrentVersion;
        saveOut.LastDailyReset = game.LastDailyReset;
        saveOut.DailyQuests = game.DailyQuests;
        saveOut.Quests = game.Quests;
        saveOut.TotalXP = game.TotalXP;

        string data = JsonSerializer.Serialize(saveOut);

        string savePath = Path.Combine(dataFolder, "quests.json");
        string backupPath = Path.Combine(dataFolder, "quests.backup.json");

        if (File.Exists(savePath))
        {
            //copies quests.json as-is, pastes in new file quests.backup.json
            //overwrite: true, meaning to replace last time's backup
            File.Copy(savePath, backupPath, overwrite: true);
        }
        File.WriteAllText(savePath, data);
    }

    public SaveData LoadFromDefinitions()
    {
        try
        {
            string questDef = Path.Combine(dataFolder, "questdefinitions.json");

            string questDefinitions = File.ReadAllText(questDef);
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
            string dQuestDef = Path.Combine(dataFolder, "dailyquestdefinitions.json");

            string dailyQuestDefinitions = File.ReadAllText(dQuestDef);
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