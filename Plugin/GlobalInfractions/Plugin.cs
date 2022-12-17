using System.Collections.Concurrent;
using GlobalInfractions.Enums;
using GlobalInfractions.Models;
using SharedLibraryCore;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions;

public class Plugin : IPlugin
{
    public const string PluginName = "Global Bans";
    public string Name => PluginName;
    public float Version => 20221213f;
    public string Author => "Amos";


    private readonly IConfigurationHandler<Configuration> _configurationHandler;
    public static Configuration Configuration = null!;
    public readonly InfractionManager InfractionManager;
    public static IManager Manager = null!;
    public static bool Active { get; private set; }

    public Plugin(IServiceProvider serviceProvider, IConfigurationHandler<Configuration> configurationHandler)
    {
        InfractionManager = new InfractionManager(serviceProvider);
        _configurationHandler = configurationHandler;
    }

    

    public async Task OnEventAsync(GameEvent gameEvent, Server server)
    {
        switch (gameEvent.Type)
        {
            case GameEvent.EventType.Join:
                await InfractionManager.UpdateProfile(gameEvent.Origin);
                break;
            case GameEvent.EventType.Disconnect:
                break;
            case GameEvent.EventType.Warn:
                await InfractionManager.NewInfraction(InfractionType.Warn, gameEvent.Origin, gameEvent.Target, gameEvent.Data);
                break;
            case GameEvent.EventType.Kick:
                await InfractionManager.NewInfraction(InfractionType.Kick, gameEvent.Origin, gameEvent.Target, gameEvent.Data);
                break;
            case GameEvent.EventType.TempBan:
                await InfractionManager.NewInfraction(InfractionType.TempBan, gameEvent.Origin, gameEvent.Target, gameEvent.Data,
                    duration: (TimeSpan)gameEvent.Extra);
                break;
            case GameEvent.EventType.Ban:
                await InfractionManager.NewInfraction(InfractionType.Ban, gameEvent.Origin, gameEvent.Target, gameEvent.Data);
                break;
            case GameEvent.EventType.Unban:
                await InfractionManager.NewInfraction(InfractionType.Unban, gameEvent.Origin, gameEvent.Target, gameEvent.Data);
                break;
        }
    }


    public async Task OnLoadAsync(IManager manager)
    {
        Console.WriteLine($"[{PluginName}] Global Bans plugin started");
        Manager = manager;

        // Check activation status
        Active = await InfractionManager.UpdateInstance();

        // Build configuration
        await _configurationHandler.BuildAsync();
        if (_configurationHandler.Configuration() == null)
        {
            Console.WriteLine($"[{PluginName}] Configuration not found, creating.");
            _configurationHandler.Set(new Configuration());
            await _configurationHandler.Save();
            await _configurationHandler.BuildAsync();
        }
        else
        {
            await _configurationHandler.Save();
        }

        Configuration = _configurationHandler.Configuration();
        Console.WriteLine($"[{PluginName}] loaded successfully. Version: {Version}");
    }

    public Task OnUnloadAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnTickAsync(Server server)
    {
        return Task.CompletedTask;
    }
}
