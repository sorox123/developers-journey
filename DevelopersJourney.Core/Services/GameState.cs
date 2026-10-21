using System.Net;
using DevelopersJourney.Models;

namespace DevelopersJourney.Services;

public class GameState
{
    public List<Quest> Quests { get; set; }
    public List<DailyQuest> DailyQuests { get; set; }
    public int TotalXP { get; set; }
    public DateTime LastDailyReset { get; set; }
    public Dictionary<string, ProjectStats> Projects { get; set; }

    public GameState(SaveData saveData)
    {
        Quests = saveData.Quests;
        DailyQuests = saveData.DailyQuests;
        TotalXP = saveData.TotalXP;
        LastDailyReset = saveData.LastDailyReset;
        Projects = new Dictionary<string, ProjectStats>();
    }

    public void CheckForReset()
    {
        DateTime now = DateTime.Now; //gets current time
        DateTime resetTime = DateTime.Today.AddHours(2); //keeps reset time as 2am on the current day

        if (now < resetTime) //if now is less than reset time, reset time was a day prior
        {
            resetTime = resetTime.AddDays(-1);
        }

        if (LastDailyReset < resetTime) //if lastdailyreset is less than resettime, reset everything
        {
            for (int i = 0; i < DailyQuests.Count; i++)
            {
                DailyQuests[i].IsComplete = false;
                DailyQuests[i].Progress = 0;
            }
            LastDailyReset = resetTime;
        }
    }

    public int CountPushes(List<GitHubEvent> events)
    {
        int totalPushes = 0; //sets pushes to 0 to start

        for (int i = 0; i < events.Count; i++) //for every event in the events list, look for Pushevent and count every one
        {
            if (events[i].Type == "PushEvent" && events[i].CreatedAt > LastDailyReset)
            {
                totalPushes++;

                // pulls and saves repo name from current event, assigning it to projectName
                string projectName = events[i].Repo.Name;

                // if the project name doesn't exist in dict, appends it as a new ProjectStats object
                if (!Projects.ContainsKey(projectName))
                {
                    Projects[projectName] = new ProjectStats();
                }

                // increments the number of pushes for that project
                Projects[projectName].ProjectPushes++;
            }
        }
        return totalPushes;
    }

    public float GetHoursWorked(WakaTimeData wakaData)
    {
        float totalTimeWorked = wakaData.Data[0].GrandTotal.TotalSeconds;
        float hoursWorked = totalTimeWorked / 3600;

        for (int i =0; i < wakaData.Data[0].Projects.Count; i++)
        {
            string projectName = wakaData.Data[0].Projects[i].Name;
            float projectTimeWorked = wakaData.Data[0].Projects[i].TotalSeconds / 3600;

            if (!Projects.ContainsKey(projectName))
            {
                Projects[projectName] = new ProjectStats();
            }

            Projects[projectName].ProjectHours = projectTimeWorked;
        }
        return hoursWorked;
    }

    public int CompleteQuest(Quest quest) //shows xp gained from quest completion an adds it to totalxp
    {
        int xp = quest.Complete();
        TotalXP += xp;
        return xp;
    }

    public int CompleteDaily(DailyQuest daily) //shows xp gained from daily quest completion and adds it to totalxp
    {
        int xp = daily.Complete();
        TotalXP += xp;
        return xp;
    }

    public int AddDailyProgress(DailyQuest daily)
    {
        if (daily.IsComplete) //if daily is complete, return 0, can't gain xp for already completed quests, exits if statement
        {
            return 0;
        }

        daily.Progress++; //increment daily's progress by 1

        if (daily.Progress >= daily.Goal) //if this completes the daily, reward xp and mark as complete
        {
            return CompleteDaily(daily);
        }
        return 0; //if incrementing the daily's progress by 1 doesn't complete it, exit the method with 0 xp

    }

    public TimeSpan TimeUntilReset()
    {
        DateTime nextReset = LastDailyReset.AddDays(1);
        return nextReset - DateTime.Now;
    }

    public void AddQuest(Quest quest)
    {
        Quests.Add(quest);
    }

    public void DeleteQuest(Quest quest)
    {
        Quests.Remove(quest);
    }
}