using SharedLibraryCore;
using SharedLibraryCore.Commands;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace BanHub.Commands;

public class WhitelistCommand : Command
{
    public WhitelistCommand(CommandConfiguration config, ITranslationLookup layout) : base(config, layout)
    {
        Name = "whitelist";
        Description = "Whitelists a global banned player";
        Alias = "wlist";
        Permission = EFClient.Permission.SeniorAdmin;
        RequiresTarget = true;
        Arguments = new[]
        {
            new CommandArgument
            {
                Name = layout["COMMANDS_ARGS_PLAYER"],
                Required = true
            }
        };
    }

    public override async Task ExecuteAsync(GameEvent gameEvent)
    {
        var clientGuid = Plugin.WhitelistedClientIds.Contains(gameEvent.Target.ClientId);

        if (clientGuid)
        {
            Plugin.WhitelistedClientIds.Remove(gameEvent.Target.ClientId);
            gameEvent.Origin.Tell($"Whitelisted: {gameEvent.Target.CleanedName}");
        }
        else
        {
            Plugin.WhitelistedClientIds.Add(gameEvent.Target.ClientId);
            gameEvent.Origin.Tell($"Removed from Whitelist: {gameEvent.Target.CleanedName}");
        }
    }
}
