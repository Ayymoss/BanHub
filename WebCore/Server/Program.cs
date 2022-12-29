using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Server.Middleware;
using GlobalInfraction.WebCore.Server.Services;
using GlobalInfraction.WebCore.Server.Utilities;
using Microsoft.EntityFrameworkCore;

// TODO: Without Server-side restart, custom auth doesn't have updated authkey list. So each activation needs a server-side restart.

SetupConfiguration.InitConfiguration();
var configuration = SetupConfiguration.ReadConfiguration();

if (configuration.DiscordHook is null) Environment.Exit(10);

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
builder.WebHost.ConfigureKestrel(options =>
{
    configuration.DatabaseType = DatabaseType.Sqlite;
    options.ListenLocalhost(8123);
});
#else
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(configuration.WebBind);
});
#endif

if (configuration.DatabaseType is DatabaseType.Sqlite)
{
    builder.Services.AddDbContext<DataContext>(options => options.UseSqlite("Data Source=GlobalBan.db"));
}
else
{
    builder.Services.AddDbContext<DataContext>(
        options =>
        {
            options.UseNpgsql($"Host={configuration.Database.HostName};" +
                              $"Port={configuration.Database.Port};" +
                              $"Username={configuration.Database.UserName};" +
                              $"Password={configuration.Database.Password};" +
                              $"Database={configuration.Database.Database}");
        });
}

builder.Services.AddLogging();

builder.Services.AddSingleton(configuration);
builder.Services.AddSingleton<ApiKeyCache>();
builder.Services.AddSingleton<PluginAuthentication>();

builder.Services.AddTransient<ApiKeyMiddleware>();

builder.Services.AddScoped<IEntityService, EntityService>();
builder.Services.AddScoped<IHeartBeatService, HeartBeatService>();
builder.Services.AddScoped<IInfractionService, InfractionService>();
builder.Services.AddScoped<IInstanceService, InstanceService>();
builder.Services.AddScoped<IDiscordWebhookService, DiscordWebhookService>();

builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen();

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
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
