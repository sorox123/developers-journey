using DevelopersJourney.Models;
using DevelopersJourney.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Text;

var storage = new SaveService();
ApiService apiService;
SaveData saveData;
bool githubFetchSuccess = false;
bool wakaFetchSuccess = false;
Dictionary<string, ProjectStats> projectStats = new Dictionary<string , ProjectStats>();

try
{
    saveData = storage.LoadGame();
}
catch (Exception ex)
{
    Console.WriteLine("Error loading game: " + ex.Message);
    return;
}

var game = new GameState(saveData);

try //checks for secrets and creates an ApiService object if there is one, returns error if not
{
    apiService = new ApiService();
}
catch (Exception ex)
{
    Console.WriteLine("Error fetching API data: " + ex.Message);
    return;
}

List<GitHubEvent> pushData = null; //init null GitHubEvent list to hold pushData
try
{
    pushData = await apiService.GetGitHubPushes();
    githubFetchSuccess = true;
}
catch (Exception ex)
{
    Console.WriteLine("Error fetching GitHub data: " + ex.Message);
}

WakaTimeData wakaTimeData = null; //init null WakaTimeData object to hold wakaTimeData
try
{
    wakaTimeData = await apiService.GetWakaTimeData();
    wakaFetchSuccess = true;
}
catch (Exception ex)
{
    Console.WriteLine("Error fetching WakaTime data: " + ex.Message);
}

List<Quest> quests = saveData.Quests;
int totalXP = saveData.TotalXP;
List<DailyQuest> dailyQuests = saveData.DailyQuests;
DateTime lastDailyReset = saveData.LastDailyReset;

int totalPushes = 0;

float hoursWorked = 0;

if (githubFetchSuccess) 
{
    List<GitHubEvent> events = pushData;
    for (int i = 0; i < events.Count; i++)
    {
        if (events[i].Type == "PushEvent" && events[i].CreatedAt > lastDailyReset)
        {
            totalPushes++;

            // pulls and saves repo name from current event
            string projectName = events[i].Repo.Name;

            // if the project name doesn't exist in dict, appends it as a new ProjectStats object
            if (!projectStats.ContainsKey(projectName))
            {
                projectStats[projectName] = new ProjectStats();
            }

            // increments the number of pushes for that project
            projectStats[projectName].ProjectPushes++;
        }
    }
}


if (wakaFetchSuccess)
{
    WakaTimeData wakaTimeResponse = wakaTimeData;
    float totalTimeWorked = wakaTimeResponse.Data[0].GrandTotal.TotalSeconds;
    hoursWorked = totalTimeWorked / 3600;

    for (int i =0; i < wakaTimeResponse.Data[0].Projects.Count; i++)
    {
        string projectName = wakaTimeResponse.Data[0].Projects[i].Name;
        float projectTimeWorked = wakaTimeResponse.Data[0].Projects[i].TotalSeconds / 3600;

        if (!projectStats.ContainsKey(projectName))
        {
            projectStats[projectName] = new ProjectStats();
        }

        projectStats[projectName].ProjectHours = projectTimeWorked;
    }
}

for (int i = 0; i < dailyQuests.Count; i++)
{
    if (!dailyQuests[i].IsComplete) // checks to see if daily hasn't already been completed
    {
        if (dailyQuests[i].Title == "Make 2 pushes to GitHub")
        {
            dailyQuests[i].Progress = totalPushes;
            if (dailyQuests[i].Progress >= dailyQuests[i].Goal)
            {
                int dailyXP = dailyQuests[i].Complete();
                Console.WriteLine(dailyQuests[i].Title + " completed! +" + dailyXP + " XP");
                totalXP = totalXP + dailyXP;
            }
        }
        if (dailyQuests[i].Title == "Actively code in VSCode for 5 hours")
        {
            dailyQuests[i].Progress = (int)hoursWorked;
            if (dailyQuests[i].Progress >= dailyQuests[i].Goal)
            {
                int dailyXP = dailyQuests[i].Complete();
                Console.WriteLine(dailyQuests[i].Title + " completed! +" + dailyXP + " XP");
                totalXP = totalXP + dailyXP;
            }
        }
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

    Console.WriteLine("Daily Quests:");

    //Iterate through dailyQuests and print them AFTER regular quests
    for (int i = 0; i < dailyQuests.Count; i++)
    {
        Console.WriteLine((i + 1) + ". " + dailyQuests[i].Title + " - Progress: " + dailyQuests[i].Progress + "/" + dailyQuests[i].Goal);
    }

    Console.WriteLine("Which quest number did you complete?");
    string input = Console.ReadLine();

    // checks if input is "quit" or "exit"
    if (input == "quit" || input == "exit")
    {
        keepPlaying = false;
        continue;
    }

    try
    {
        if (input.StartsWith("daily"))
        {
            int chosenDaily = int.Parse(input.Substring(6));
            DailyQuest dailyProgression = dailyQuests[chosenDaily - 1];

            //checks to make sure the daily isn't already complete.
            if (dailyProgression.IsComplete == true)
            {
                Console.WriteLine("Daily quest is already completed.");
                continue;
            }

            dailyProgression.Progress++;

            if (dailyProgression.Progress >= dailyProgression.Goal)
            {
                int dailyXP = dailyProgression.Complete();
                Console.WriteLine(dailyProgression.Title + " completed! +" + dailyXP + " XP");
                totalXP = totalXP + dailyXP;
            }
            
            Console.WriteLine(dailyProgression.Title + " progress: " + dailyProgression.Progress + " / " + dailyProgression.Goal);
        }
        else
        {
            int chosenNumber = int.Parse(input);
            Quest chosenQuest = quests[chosenNumber - 1];
            if (chosenQuest.IsComplete)
            {
                Console.WriteLine("Quest is already completed.");
                continue;
            }
            int earnedXP = chosenQuest.Complete();
            Console.WriteLine(chosenQuest.Title + " completed! +" + earnedXP + " XP");
            totalXP = totalXP + earnedXP;
            Console.WriteLine("Total XP: " + totalXP);
        }
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

try
{
    storage.SaveGame(quests, totalXP, dailyQuests, lastDailyReset);
    Console.WriteLine("Game saved successfully.");
}
catch (Exception ex)
{
    Console.WriteLine("Error saving game: " + ex.Message);
}