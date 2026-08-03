using System.Text.Json;
using System.IO;

List<Quest> quests = new();
int totalXP = 0;

if (File.Exists("quests.json"))
{
    string json = File.ReadAllText("quests.json"); // checks save file
    SaveData save = JsonSerializer.Deserialize<SaveData>(json); // passes save file here
    quests = save.Quests;
    totalXP = save.TotalXP;
}
else
{
    Quest quest1 = new Quest(); // the first item of the "quests" list, filled with attributes of Quest class
    quest1.Title = "Ship a small C# application";
    quest1.IsComplete = false;
    quest1.XP = 50;
    quests.Add(quest1);

    Quest quest2 = new Quest();
    quest2.Title = "Create a professional README";
    quest2.IsComplete = false;
    quest2.XP = 30;
    quests.Add(quest2);

}



bool keepPlaying = true;



while (keepPlaying)
{
    for (int i = 0; i < quests.Count; i++) // iterates over every quest and assigns it a number
    {
        Console.WriteLine((i + 1) + ". " + quests[i].Title + " - Complete: " + quests[i].IsComplete);
    }
    Console.WriteLine( "Which quest number did you complete?");
    string input = Console.ReadLine(); // reads user input

    try
    {
        int chosenNumber = int.Parse(input); // assigns int chosenNumber to user input
        Quest chosenQuest = quests[chosenNumber -1];
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
// have to redefine a SaveData class object with a different name because I hate myself and I am not using wrappers for top-end objects for some reason.
SaveData saveOut = new SaveData();
saveOut.Quests = quests;
saveOut.TotalXP = totalXP;


string data = JsonSerializer.Serialize(saveOut); // write to the save file
File.WriteAllText("quests.json", data);

class Quest // class Quest consists of its title and completion state, 
{
    public string Title { get; set; }
    public bool IsComplete { get; set; }
    public int XP { get; set; }

    public int Complete() // sets IsComplete to true, prints message and returns int XP
    {
        IsComplete = true;
        Console.WriteLine(Title + " marked complete! +" + XP + " XP");
        return XP;
    }
}
// holds both quests and totalxp
class SaveData
{
    public List<Quest> Quests { get; set; }
    public int TotalXP { get; set; }
}