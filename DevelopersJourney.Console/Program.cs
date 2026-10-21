using DevelopersJourney.Models;
using DevelopersJourney.Services;

string dataFolder = "../Data";
var storage = new SaveService(dataFolder); //pulls SaveService files from Data and keeps it as storage var
ApiService apiService;
SaveData saveData;
bool githubFetchSuccess = false;
bool wakaFetchSuccess = false;

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
    apiService = new ApiService(dataFolder); //pulls ApiService files from /Data as apiService
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

game.CheckForReset();

int totalPushes = 0;

float hoursWorked = 0;

if (githubFetchSuccess) 
{
    totalPushes = game.CountPushes(pushData);  
}


if (wakaFetchSuccess)
{
    hoursWorked = game.GetHoursWorked(wakaTimeData);
}

foreach (DailyQuest daily in game.DailyQuests)
{
    if (!daily.IsComplete) // checks to see if daily hasn't already been completed
    {
        if (daily.Source == ProgressSource.GitHubPushes)
        {
            daily.Progress = totalPushes;
            if (daily.Progress >= daily.Goal)
            {
                int dailyXP = game.CompleteDaily(daily);
                Console.WriteLine(daily.Title + " completed! +" + dailyXP + " XP");
            }
        }
        if (daily.Source == ProgressSource.CodingHours)
        {
            daily.Progress = (int)hoursWorked;
            if (daily.Progress >= daily.Goal)
            {
                int dailyXP = game.CompleteDaily(daily);
                Console.WriteLine(daily.Title + " completed! +" + dailyXP + " XP");
            }
        }
    }
}

bool keepPlaying = true;


while (keepPlaying)
{
    Console.WriteLine("Filter by tag (or press enter to see all):");

    string filterTag = Console.ReadLine();
    int qi = 0;

    foreach (Quest quest in game.Quests)
    {
        qi++;
        if (filterTag != "" && !quest.SkillTags.Contains(filterTag)) // skip if user hits enter. if filter was typed and this quest doesn't have that tag
        {
            continue; // skip rest of loop iteration
        }

        string tags = string.Join(", ", quest.SkillTags);
        Console.WriteLine(qi + ". " + quest.Title + " - Complete: " + quest.IsComplete + " [" + tags + "]");
    }

    Console.WriteLine("Daily Quests:");

    int di = 0;
    //Iterate through dailyQuests and print them AFTER regular quests
    foreach (DailyQuest daily in game.DailyQuests)
    {
        di++;
        Console.WriteLine(di + ". " + daily.Title + " - Progress: " + daily.Progress + "/" + daily.Goal);
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
            DailyQuest dailyProgression = game.DailyQuests[chosenDaily - 1];

            //checks to make sure the daily isn't already complete.
            if (dailyProgression.IsComplete == true)
            {
                Console.WriteLine("Daily quest is already completed.");
                continue;
            }

            int dailyXP = game.AddDailyProgress(dailyProgression);
            
            if (dailyXP > 0 )
            {
                Console.WriteLine($"{dailyProgression.Title} completed! +{dailyXP}XP");
            }
            else
            {
                Console.WriteLine(dailyProgression.Title + " progress: " + dailyProgression.Progress + " / " + dailyProgression.Goal);
            }
        }
        else
        {
            int chosenNumber = int.Parse(input);
            Quest chosenQuest = game.Quests[chosenNumber - 1];
            if (chosenQuest.IsComplete)
            {
                Console.WriteLine("Quest is already completed.");
                continue;
            }
            int earnedXP = game.CompleteQuest(chosenQuest);
            Console.WriteLine(chosenQuest.Title + " completed! +" + earnedXP + " XP");
            Console.WriteLine("Total XP: " + game.TotalXP);
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
    storage.SaveGame(game);
    Console.WriteLine("Game saved successfully.");
}
catch (Exception ex)
{
    Console.WriteLine("Error saving game: " + ex.Message);
}