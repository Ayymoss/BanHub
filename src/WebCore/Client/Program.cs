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
using Radzen;

// TODO: LOGGING!!!!
// TODO: After restructure need to move imports back to imports... 2023-09-21

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
builder.Services.AddSingleton<TomatoCounterHub>();

builder.Services.AddSingleton<CommunityService>();
builder.Services.AddSingleton<NoteService>();
builder.Services.AddSingleton<PenaltyService>();
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSingleton<PlayerProfileService>();
builder.Services.AddSingleton<PlayersService>();
builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<ServerService>();
builder.Services.AddSingleton<StatisticService>();

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
