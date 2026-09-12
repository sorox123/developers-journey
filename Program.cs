using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Text;

SaveData saveData = LoadGame();
List<Quest> quests = saveData.Quests;
int totalXP = saveData.TotalXP;
List<DailyQuest> dailyQuests = saveData.DailyQuests;
DateTime lastDailyReset = saveData.LastDailyReset;
DateTime resetTime = DateTime.Today.AddHours(2);
bool githubFetchSuccess = false;
bool wakaFetchSuccess = false;
Dictionary<string, ProjectStats> projectStats = new Dictionary<string , ProjectStats>();


//deserialize the contents of secrets.json
static Secrets LoadSecrets()
{
    string json = File.ReadAllText("secrets.json");
    Secrets secrets = JsonSerializer.Deserialize<Secrets>(json);
    return secrets;
}

if (lastDailyReset < resetTime)
{
    for (int i = 0; i < dailyQuests.Count; i++)
    {
        dailyQuests[i].IsComplete = false;
        dailyQuests[i].Progress = 0;
    }
    lastDailyReset = resetTime;
}

static SaveData LoadFromDefinitions()
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

//returns "task" string. "task" signifies the result may not be ready yet but to anticipate it as a string
static async Task<string> GetGitHubPushes(string token)
{
    //HttpClient makes web requests
    HttpClient client = new HttpClient();
    // identify app and attach token
    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
    client.DefaultRequestHeaders.Add("User-Agent", "DevelopersJourney");

    //GetAsync fetches the the repo url. Sicne this takes time, we use await
    HttpResponseMessage response = await client.GetAsync("https://api.github.com/users/sorox123/events");
    //sets the HttpResponseMessage as a string before we return it
    string result = await response.Content.ReadAsStringAsync();

    return result;
}

static async Task<string> GetWakaTimeData(string token)
{
    HttpClient client = new HttpClient();

    // turns API key string into raw bytes since Base64 works on bytes, not strings.
    string encodedToken = Convert.ToBase64String(Encoding.ASCII.GetBytes(token));
    // converts the raw bytes into a string that can be used in the header. The "Basic" part is required by WakaTime API.
    client.DefaultRequestHeaders.Add("Authorization", "Basic " + encodedToken);

    client.DefaultRequestHeaders.Add("User-Agent", "DevelopersJourney");
    //fetches time use for today on this particular project.
    HttpResponseMessage response = await client.GetAsync("https://wakatime.com/api/v1/users/current/summaries?start=today&end=today");
    string result = await response.Content.ReadAsStringAsync();

    return result;
}


string githubToken = LoadSecrets().GitHubToken;
string pushData = "";
int totalPushes = 0;

try
{
    pushData = await GetGitHubPushes(githubToken);
    githubFetchSuccess = true;
}
catch
{
    Console.WriteLine("Couldn't reach GitHub. Check internet connection and try again.");
    githubFetchSuccess = false;
}

float hoursWorked = 0;

if (githubFetchSuccess) 
{
    List<GitHubEvent> events = JsonSerializer.Deserialize<List<GitHubEvent>>(pushData);
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


string wakaTimeToken = LoadSecrets().WakaTimeToken;
string wakaTimeData = "";

try
{
    wakaTimeData = await GetWakaTimeData(wakaTimeToken);
    wakaFetchSuccess = true;
}
catch
{
    Console.WriteLine("Couldn't reach WakaTime. Check internet connection and try again.");
    wakaFetchSuccess = false;
}

if (wakaFetchSuccess)
{

    WakaTimeData wakaTimeResponse = JsonSerializer.Deserialize<WakaTimeData>(wakaTimeData);
    float totalTimeWorked = wakaTimeResponse.Data[0].GrandTotal.TotalSeconds;
    hoursWorked = totalTimeWorked / 3600;
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
                totalXP = totalXP + dailyXP;
            }
        }
        if (dailyQuests[i].Title == "Actively code in VSCode for 5 hours")
        {
            dailyQuests[i].Progress = (int)hoursWorked;
            if (dailyQuests[i].Progress >= dailyQuests[i].Goal)
            {
                int dailyXP = dailyQuests[i].Complete();
                totalXP = totalXP + dailyXP;
            }
        }
    }
}


static List<DailyQuest> LoadDailyQuestFromDefinitions()
{
    try
    {
        string dailyQuestDefinitions = File.ReadAllText("dailyquestdefinitions.json");
        List<DailyQuest> dailyQuests = JsonSerializer.Deserialize<List<DailyQuest>>(dailyQuestDefinitions);

        return dailyQuests;
    }
    catch (Exception Dex)
    {
        Console.WriteLine("Fatal error: could not load daily quest definitions");
        Console.WriteLine(Dex.Message);
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
    SaveGame(quests, totalXP, dailyQuests, lastDailyReset);
    Console.WriteLine("Game saved successfully.");
}
catch (Exception ex)
{
    Console.WriteLine("Error saving game: " + ex.Message);
}

static SaveData LoadGame()
{
    if (File.Exists("quests.json"))
    {
        try
        {
            string json = File.ReadAllText("quests.json");
            SaveData save = JsonSerializer.Deserialize<SaveData>(json);
            if (save.DailyQuests == null)
            {
                save.DailyQuests = LoadDailyQuestFromDefinitions();
            }
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

static void SaveGame(List<Quest> quests, int totalXP, List<DailyQuest> dailyQuests, DateTime lastDailyReset)
{
    SaveData saveOut = new SaveData();
    saveOut.LastDailyReset = lastDailyReset;
    saveOut.DailyQuests = dailyQuests;
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

class DailyQuest : Quest
{
    public int Progress { get; set; } // progress "how many pushes to github today"
    public int Goal { get; set; } // goal "out of this many pushes to github"
}

class SaveData
{
    public List<Quest> Quests { get; set; }
    public List<DailyQuest> DailyQuests { get; set; }
    public int TotalXP { get; set; }
    public DateTime LastDailyReset { get; set; }
}

//stores tokens here (defined in gitignore)
class Secrets
{
    public string GitHubToken { get; set; }
    public string WakaTimeToken { get; set; }
}

// parses the name field of the json response
class GitHubRepo
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

// parses json response from api
class GitHubEvent
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("repo")]
    public GitHubRepo Repo { get; set; }
}

class WakaTimeData
{
    [JsonPropertyName("data")]
    public List<WakaTimeGrandTotal> Data { get; set; }
}

class WakaTimeGrandTotal
{
    [JsonPropertyName("grand_total")]
    public WakaTimeTotalSeconds GrandTotal { get; set; }
}

class WakaTimeTotalSeconds
{
    [JsonPropertyName("total_seconds")]
    public float TotalSeconds { get; set; }
}

class ProjectStats
{
    public int ProjectPushes { get; set; }
    public float ProjectHours { get; set; }
}
