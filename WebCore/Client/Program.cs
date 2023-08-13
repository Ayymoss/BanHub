using BanHub.WebCore.Client;
using BanHub.WebCore.Client.Handlers;
using BanHub.WebCore.Client.Providers;
using BanHub.WebCore.Client.Services.RestEase;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Client.SignalR;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using MudBlazor.Services;

// TODO: LOGGING!!!!

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

#if DEBUG
builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});
#else
builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri("https://banhub.gg/")});
#endif

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CookieHandler>();

builder.Services.AddSingleton<ActiveUserHub>();
builder.Services.AddSingleton<StatisticsHub>();
builder.Services.AddSingleton<CommunityService>();
builder.Services.AddSingleton<NoteService>();
builder.Services.AddSingleton<PenaltyService>();
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSingleton<PlayerProfileService>();
builder.Services.AddSingleton<PlayersService>();
builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<ServerService>();
builder.Services.AddSingleton<StatisticService>();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Text;
});

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
