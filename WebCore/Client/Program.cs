using BanHub.WebCore.Client;
using BanHub.WebCore.Client.Handlers;
using BanHub.WebCore.Client.Interfaces;
using BanHub.WebCore.Client.Providers;
using BanHub.WebCore.Client.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

#if DEBUG
builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});
#else
builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri("https://banhub.gg/")});
#endif

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<CookieHandler>();

builder.Services.AddMudServices();
builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
