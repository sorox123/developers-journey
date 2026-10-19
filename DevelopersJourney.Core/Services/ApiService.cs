using DevelopersJourney.Models;
using System.Text.Json;
using System.Text;
using System.Net;

namespace DevelopersJourney.Services;

public class ApiService
{
    //Declare fields
    private HttpClient client;
    private Secrets secrets;

    //Construct fields
    public ApiService()
    {
        //creates a new HttpClient object with a User-Agent header
        client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "DevelopersJourney");
        //creates a new Secrets object with content from secrets.json
        string json = File.ReadAllText("secrets.json");
        secrets = JsonSerializer.Deserialize<Secrets>(json);
    }

    public async Task<List<GitHubEvent>> GetGitHubPushes()
    {
        var gitRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/users/sorox123/events"); //new request for github info
        gitRequest.Headers.Add("Authorization", "Bearer " + secrets.GitHubToken); //params required; authorization, bearer + token
        HttpResponseMessage gitResponse = await client.SendAsync(gitRequest); //await respons from github
        //if the response was not successful, throw an exception with clear message (include the status code)
        if (!gitResponse.IsSuccessStatusCode)
        {
            throw new Exception($"GitHub API request failed with status code: {gitResponse.StatusCode}");
        }

        //sets the HttpResponseMessage as a string before we return it
        string gitResult = await gitResponse.Content.ReadAsStringAsync();

        //sets the HttpResponseMessage as a List before we return it
        List<GitHubEvent> pushData = JsonSerializer.Deserialize<List<GitHubEvent>>(gitResult);

        if (pushData == null)
        {
            throw new Exception("GitHub response is null.");
        }
        
        return pushData;
    }

    public async Task<WakaTimeData> GetWakaTimeData()
    {
        string encodedToken = Convert.ToBase64String(Encoding.ASCII.GetBytes(secrets.WakaTimeToken)); //turns API key string into raw bytes since Base64 works on bytes, not strings.
        var wakaRequest = new HttpRequestMessage(HttpMethod.Get, "https://wakatime.com/api/v1/users/current/summaries?start=today&end=today"); //new request for waka info
        wakaRequest.Headers.Add("Authorization", "Basic " + encodedToken); //params required; authorization, auth level, token
        HttpResponseMessage wakaResponse = await client.SendAsync(wakaRequest); //await response from waka
        //if the response was not successful, throw an exception with clear message (include status code)
        if (!wakaResponse.IsSuccessStatusCode) //checks for lack of success status code
        {
            throw new Exception($"WakaTime API request failed with status code: {wakaResponse.StatusCode}");
        }
        
        //sets HttpResponseMessage as a string before we return it
        string wakaResult = await wakaResponse.Content.ReadAsStringAsync();

        WakaTimeData wakaData = JsonSerializer.Deserialize<WakaTimeData>(wakaResult);

        //check to see if serializer returns anything
        if (wakaData == null)
        {
            throw new Exception("WakaTime response is null.");
        }

        //check to see if the data is missing or empty
        if (wakaData.Data == null || wakaData.Data.Count == 0)
        {
            throw new Exception("WakaTime returned no data for today.");   
        }

        return wakaData;
    }
}