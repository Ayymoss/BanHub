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
    
    // Configuration??

    public readonly InfractionManager InfractionManager;
    public static IManager Manager = null!;

    public Plugin(IServiceProvider serviceProvider)
    {
        InfractionManager = new InfractionManager(serviceProvider);
    }

    public bool Active { get; set; }

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
                break;
            case GameEvent.EventType.WarnClear:
                break;
            case GameEvent.EventType.Kick:
                Console.WriteLine($"DATA: {gameEvent.Data}");
                await InfractionManager.NewInfraction(InfractionType.Kick, gameEvent.Origin, gameEvent.Target, gameEvent.Data);
                break;
            case GameEvent.EventType.TempBan:
                break;
            case GameEvent.EventType.Ban:
                break;
            case GameEvent.EventType.Unban:
                break;
        }
    }



    public async Task OnLoadAsync(IManager manager)
    {
        Manager = manager;
        Console.WriteLine($"[{PluginName}] Global Bans plugin started");
        Active = await InfractionManager.UpdateInstance();
        Console.WriteLine($"[{PluginName}] Global Bans plugin loaded");
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
