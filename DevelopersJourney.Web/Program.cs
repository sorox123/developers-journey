using DevelopersJourney.Models;
using DevelopersJourney.Services;
using DevelopersJourney.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
string dataFolder = "../Data";

//create shared game objects
SaveService storage = new SaveService(dataFolder);
SaveData saveData = storage.LoadGame();
GameState game = new GameState(saveData);

builder.Services.AddSingleton(storage);
builder.Services.AddSingleton(game);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
