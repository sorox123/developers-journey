namespace DevelopersJourney.Models;

public class Quest
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