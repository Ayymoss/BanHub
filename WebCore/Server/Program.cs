using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Middleware;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Server.SignalR;
using BanHub.WebCore.Server.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Radzen;
using Serilog;
using Serilog.Events;

// TODO: LOGGING!!!!

SetupConfiguration.InitConfigurationAsync();
var configuration = SetupConfiguration.ReadConfiguration();

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
builder.WebHost.ConfigureKestrel(options => { options.ListenLocalhost(8123); });
#else
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(configuration.WebBind, configure => configure.UseHttps()); });
#endif

// TODO: TOGGLE MANUALLY - Migrations don't seem to honour build state
configuration.Database.Database = "LiveTestBanHub-Dev";

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql($"Host={configuration.Database.HostName};" +
                      $"Port={configuration.Database.Port};" +
                      $"Username={configuration.Database.UserName};" +
                      $"Password={configuration.Database.Password};" +
                      $"Database={configuration.Database.Database}");
});

builder.Services.AddSingleton(configuration);
builder.Services.AddSingleton<IPluginAuthenticationCache, PluginAuthenticationCache>();
builder.Services.AddSingleton<SignedInUsers>();
builder.Services.AddSingleton<PluginAuthentication>();
builder.Services.AddSingleton<StatisticsCache>();
builder.Services.AddSingleton<ISentimentService, SentimentService>();
builder.Services.AddSingleton<ICommunityConnectionManager, CommunityConnectionManager>();

builder.Services.AddTransient<ApiKeyMiddleware>();

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("./Account/"))
    .SetApplicationName("BanHubAccount");

// Add services to the container.
builder.Services.AddLogging();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen(genOptions =>
{
    genOptions.SwaggerDoc("v1", new OpenApiInfo {Title = "BanHub API", Version = "v1"});
    genOptions.CustomSchemaIds(type => type.FullName);
});
builder.Services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(Program).Assembly); });

if (!Directory.Exists(Path.Join(AppContext.BaseDirectory, "Log")))
    Directory.CreateDirectory(Path.Join(AppContext.BaseDirectory, "Log"));

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Join(AppContext.BaseDirectory, "Log", "banhub-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 10,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.Cookie.Name = "BanHubAccount";
        opt.LogoutPath = "/";
        opt.LoginPath = "/";
        opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        opt.Cookie.SameSite = SameSiteMode.Strict;
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

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // TODO: Move Swagger to dev only
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
app.MapHub<PluginHub>("/SignalR/PluginHub");
app.MapHub<ActiveUserHub>("/SignalR/ActiveUsersHub");
app.MapHub<StatisticsHub>("/SignalR/StatisticsHub");
app.MapHub<TomatoCounterHub>("/SignalR/TomatoCounterHub");

app.UseCors("CorsSpecs");
app.UseAuthentication();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");


app.Run();
