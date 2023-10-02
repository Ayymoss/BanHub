using BanHub.Application.DTOs.WebView.CommunityProfileView;
using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Application.Interfaces;
using BanHub.Application.Interfaces.Services;
using BanHub.Application.Mediatr.Community.Commands;
using BanHub.Application.Mediatr.Penalty.Commands;
using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Application.Utilities;
using BanHub.Domain.Interfaces;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.Interfaces.Services;
using BanHub.Infrastructure.Context;
using BanHub.Infrastructure.Repositories;
using BanHub.Infrastructure.Repositories.Pagination;
using BanHub.Infrastructure.Services;
using BanHub.Infrastructure.SignalR.Hubs;
using BanHub.WebCore.Server.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Radzen;
using Serilog;
using Serilog.Events;
using Community = BanHub.Application.DTOs.WebView.CommunitiesView.Community;
using Penalty = BanHub.Application.DTOs.WebView.PlayerProfileView.Penalty;

// TODO: LOGGING!!!!

#region Builder

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

#region Service Registration

#region Singletons

builder.Services.AddSingleton(configuration);
builder.Services.AddSingleton<IPluginAuthenticationCache, PluginAuthenticationCache>();
builder.Services.AddSingleton<ISignedInUsersManager, SignedInUsersManager>();
builder.Services.AddSingleton<PluginAuthentication>();
builder.Services.AddSingleton<IStatisticsCache, StatisticsCache>();
builder.Services.AddSingleton<ISentimentModelCache, SentimentModelCache>();
builder.Services.AddSingleton<ICommunityConnectionManager, CommunityConnectionManager>();

#endregion

#region Transients

builder.Services.AddTransient<PluginVersionCheckMiddleware>();
builder.Services.AddTransient<ApiKeyMiddleware>();

#endregion

#region Scoped

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddScoped<ISignalRNotification, SignalRNotificationFactory>();

#region Scoped - Repositories

// Context Repositories
builder.Services.AddScoped<IAliasRepository, AliasRepository>();
builder.Services.AddScoped<IAuthTokenRepository, AuthTokenRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatSentimentRepository, ChatSentimentRepository>();
builder.Services.AddScoped<ICommunityRepository, CommunityRepository>();
builder.Services.AddScoped<ICurrentAliasRepository, CurrentAliasRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<IPenaltyIdentifierRepository, PenaltyIdentifierRepository>();
builder.Services.AddScoped<IPenaltyRepository, PenaltyRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IServerConnectionRepository, ServerConnectionRepository>();
builder.Services.AddScoped<IServerRepository, ServerRepository>();

#endregion

#region Scoped - Pagination Query Helpers

builder.Services.AddScoped<IResourceQueryHelper<GetProfilePenaltiesPaginationCommand, Penalty>, ProfilePenaltiesPaginationQueryHelper>();
builder.Services.AddScoped<IResourceQueryHelper<GetProfileNotesPaginationCommand, Note>, ProfileNotesPaginationQueryHelper>();
builder.Services.AddScoped<IResourceQueryHelper<GetProfileChatPaginationCommand, Chat>, ProfileChatPaginationQueryHelper>();
builder.Services.AddScoped<IResourceQueryHelper<GetCommunitiesPaginationCommand, Community>, CommunitiesPaginationQueryHelper>();
builder.Services
    .AddScoped<IResourceQueryHelper<GetCommunityProfileServersPaginationCommand, Server>,
        CommunityProfileServersPaginationQueryHelper>();
builder.Services
    .AddScoped<IResourceQueryHelper<GetPlayersPaginationCommand, BanHub.Application.DTOs.WebView.PlayersView.Player>,
        PlayersPaginationQueryHelper>();
builder.Services
    .AddScoped<IResourceQueryHelper<GetProfileConnectionsPaginationCommand, Connection>,
        ProfileConnectionsPaginationQueryHelper>();
builder.Services
    .AddScoped<IResourceQueryHelper<GetPenaltiesPaginationCommand, BanHub.Application.DTOs.WebView.PenaltiesView.Penalty>,
        PenaltiesPaginationQueryHelper>();
builder.Services
    .AddScoped<IResourceQueryHelper<GetCommunityProfilePenaltiesPaginationCommand, BanHub.Application.DTOs.WebView.CommunityProfileView.Penalty>,
        CommunityProfilePenaltiesPaginationQueryHelper>();

#endregion

#endregion

#endregion

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
#if DEBUG
    .MinimumLevel.Information()
    .MinimumLevel.Override("BanHub", LogEventLevel.Debug)
#else
    .MinimumLevel.Warning()
#endif
    .Enrich.FromLogContext()
    .Enrich.With<ShortSourceContextEnricher>()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Join(AppContext.BaseDirectory, "Log", "banhub-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 10,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [{ShortSourceContext}] {Message:lj}{NewLine}{Exception}")
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

#endregion

#region App

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // TODO: Move Swagger to dev only
    app.UseWebAssemblyDebugging();
    Generic.IsDebug = true;
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    Generic.IsDebug = false;
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
app.UseMiddleware<PluginVersionCheckMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

#endregion
