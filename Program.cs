using System.Text.Json;
using System.IO;

SaveData saveData = LoadGame();
List<Quest> quests = saveData.Quests;
int totalXP = saveData.TotalXP;
static SaveData LoadFromDefinitions()
{
    try
    {
        string questDefinitions = File.ReadAllText("questdefinitions.json");
        List<Quest> quests = JsonSerializer.Deserialize<List<Quest>>(questDefinitions);

        SaveData questsList = new SaveData();
        questsList.Quests = quests;
        questsList.TotalXP = 0;
        return questsList;
    }
    catch (Exception ex)
    {
        Console.WriteLine("Fatal error: could not load quest definitions.");
        Console.WriteLine(ex.Message);
        Environment.Exit(1);
        return null;
    }
}

bool keepPlaying = true;

while (keepPlaying)
{
    Console.WriteLine("Filter by tag (or press enter to see all):");
    string filterTag = Console.ReadLine();

    for (int i = 0; i < quests.Count; i++)
    {
        if (filterTag != "" && !quests[i].SkillTags.Contains(filterTag)) // skip if user hits enter. if filter was typed and this quest doesn't have that tag
        {
            continue; // skip rest of loop iteration
        }

        string tags = string.Join(", ", quests[i].SkillTags);
        Console.WriteLine((i + 1) + ". " + quests[i].Title + " - Complete: " + quests[i].IsComplete + " [" + tags + "]");
    }
    Console.WriteLine("Which quest number did you complete?");
    string input = Console.ReadLine();

    try
    {
        int chosenNumber = int.Parse(input);
        Quest chosenQuest = quests[chosenNumber - 1];
        if (chosenQuest.IsComplete)
        {
            Console.WriteLine("Quest is already completed.");
            continue;
        }
        int earnedXP = chosenQuest.Complete();
        totalXP = totalXP + earnedXP;
        Console.WriteLine("Total XP: " + totalXP);
    }
    catch
    {
        Console.WriteLine("That wasn't a valid quest number. Try again.");
        continue;
    }

    Console.WriteLine("Complete another quest? (yes/no)");
    string again = Console.ReadLine();
    keepPlaying = (again == "yes");
}

SaveGame(quests, totalXP);

static SaveData LoadGame()
{
    if (File.Exists("quests.json"))
    {
        try
        {
            string json = File.ReadAllText("quests.json");
            SaveData save = JsonSerializer.Deserialize<SaveData>(json);
            return save;
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

static void SaveGame(List<Quest> quests, int totalXP)
{
    SaveData saveOut = new SaveData();
    saveOut.Quests = quests;
    saveOut.TotalXP = totalXP;

    string data = JsonSerializer.Serialize(saveOut);
    File.WriteAllText("quests.json", data);
}

class Quest
{
    public string Title { get; set; }
    public bool IsComplete { get; set; }
    public int XP { get; set; }
    public List<string> SkillTags { get; set; }

    public int Complete()
    {
        IsComplete = true;
        Console.WriteLine(Title + " marked complete! +" + XP + " XP");
        return XP;
    }
}

class SaveData
{
    public List<Quest> Quests { get; set; }
    public int TotalXP { get; set; }
}