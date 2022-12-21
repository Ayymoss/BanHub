using GlobalInfractions.Enums;
using SharedLibraryCore;
using SharedLibraryCore.Commands;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Commands;

public class GlobalBanCommand : Command
{
    public GlobalBanCommand(CommandConfiguration config, ITranslationLookup layout) : base(config, layout)
    {
        Name = "globalban";
        Description = "Bans a player from all servers";
        Alias = "gban";
        Permission = EFClient.Permission.Moderator;
        RequiresTarget = true;
        Arguments = new[]
        {
            new CommandArgument
            {
                Name = layout["COMMANDS_ARGS_PLAYER"],
                Required = true
            },
            new CommandArgument
            {
                Name = layout["COMMANDS_ARGS_REASON"],
                Required = true
            }
        };
    }

    public override async Task ExecuteAsync(GameEvent gameEvent)
    {
       var result = await Plugin.InfractionManager.NewInfraction(InfractionType.Ban, gameEvent.Origin, gameEvent.Target, gameEvent
       .Data, scope: InfractionScope.Global);
       gameEvent.Origin.Tell($"[{Plugin.PluginName}] {result}");
    }
}
