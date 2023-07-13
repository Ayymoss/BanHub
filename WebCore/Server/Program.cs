using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Middleware;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Server.SignalR;
using BanHub.WebCore.Server.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;


/*
// TODO: LOGGING!!!!

Web [Post("/Instance/Instances")]
Web [Get("/Instance/{identity}")]
Plugin [Get("/Instance/Active/{identity}")]
Plugin [Post("/Instance")]

Plugin [Post("/HeartBeat/Instance")]
Plugin [Post("/HeartBeat/Players")]

Web [Post("/Note")]
Web [Delete("/Note")]
Web [Get("/Note/{identity}")]

Web [Delete("/Penalty/Delete")]
Web [Get("/Penalty/Penalties/{identity}")]
Web [Post("/Penalty/Penalties")]
Web [Get("/Penalty/Latest")]
Plugin [Post("/Penalty")]
Plugin [Patch("/Penalty/Evidence")]

Web [Get("/Player/Profile/{identity}")]
Web [Get("/Player/Profile/Connections/{identity}")]
Web [Post("/Player/Players")]
Plugin [Post("/Player")]
Plugin [Post("/Player/IsBanned")]
Plugin [Post("/Player/GetToken")]

Web [Get("/Search/{query}")]

Web [Get("/Instance/Profile/Servers/{identity}")]
Plugin [Post("/Server")]

Web [Get("/Statistic")]

 */

SetupConfiguration.InitConfigurationAsync();
var configuration = SetupConfiguration.ReadConfiguration();

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
builder.WebHost.ConfigureKestrel(options => { options.ListenLocalhost(8123); });
#else
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(configuration.WebBind); });
#endif

// TODO: TOGGLE MANUALLY - Migrations don't seem to honour build state
configuration.Database.Database = "BanHubDev";

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql($"Host={configuration.Database.HostName};" +
                      $"Port={configuration.Database.Port};" +
                      $"Username={configuration.Database.UserName};" +
                      $"Password={configuration.Database.Password};" +
                      $"Database={configuration.Database.Database}");
});

builder.Services.AddSingleton(configuration);
builder.Services.AddSingleton<ApiKeyCache>();
builder.Services.AddSingleton<SignedInUsers>();
builder.Services.AddSingleton<PluginAuthentication>();
builder.Services.AddSingleton<StatisticsTracking>();

builder.Services.AddTransient<ApiKeyMiddleware>();

builder.Services.AddScoped<IDiscordWebhookService, DiscordWebhookService>();
builder.Services.AddScoped<IStatisticService, StatisticService>();

// Add services to the container.
builder.Services.AddLogging();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(Program).Assembly); });


builder.Logging.ClearProviders().AddConsole();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.Cookie.Name = "BanHubAccount";
#if DEBUG
        opt.LogoutPath = "/";
        opt.LoginPath = "/";
#else
        opt.LogoutPath = "https://banhub.gg/";
        opt.LoginPath = "https://banhub.gg/";
#endif
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsSpecs", corsPolicyBuilder =>
    {
        corsPolicyBuilder
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<ApiKeyMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapHub<ViewerCount>("/ActiveUsersHub");

app.UseCors("CorsSpecs");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");


app.Run();
