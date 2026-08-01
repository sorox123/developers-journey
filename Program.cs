List<Quest> quests = new List<Quest>(); //defines a new list of the class Quest called "quests"

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

bool keepPlaying = true;

int totalXP = 0;

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

class Quest // class Quest consists of its title and completion state, 
{
    public string Title;
    public bool IsComplete;
    public int XP;

    public int Complete() // sets IsComplete to true, prints message and returns int XP
    {
        IsComplete = true;
        Console.WriteLine(Title + " marked complete! +" + XP + " XP");
        return XP;
    }
}