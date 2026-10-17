using DevelopersJourney.Models;
using System.Text.Json;
using System.Text;

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

    public async Task<string> GetGitHubPushes()
    {
        var gitRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/users/sorox123/events"); //new request for github info
        gitRequest.Headers.Add("Authorization", "Bearer " + secrets.GitHubToken); //params required; authorization, bearer + token
        HttpResponseMessage response = await client.SendAsync(gitRequest); //await respons from github
        //sets the HttpResponseMessage as a string before we return it
        string gitResult = await response.Content.ReadAsStringAsync();

        return gitResult;
    }

    public async Task<string> GetWakaTimeData()
    {
        string encodedToken = Convert.ToBase64String(Encoding.ASCII.GetBytes(secrets.WakaTimeToken)); //turns API key string into raw bytes since Base64 works on bytes, not strings.
        var wakaRequest = new HttpRequestMessage(HttpMethod.Get, "https://wakatime.com/api/v1/users/current/summaries?start=today&end=today"); //new request for waka info
        wakaRequest.Headers.Add("Authorization", "Basic " + encodedToken); //params required; authorization, auth level, token
        HttpResponseMessage WakaResponse = await client.SendAsync(wakaRequest); //await response from waka
        //sets HttpResponseMessage as a string before we return it
        string wakaResult = await WakaResponse.Content.ReadAsStringAsync();

        return wakaResult;
    }
}