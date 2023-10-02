using BanHub.Plugin.Managers;
using SharedLibraryCore;
using SharedLibraryCore.Commands;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace BanHub.Plugin.Commands;

public class WhitelistCommand : Command
{
    private readonly WhitelistManager _whitelistManager;

    public WhitelistCommand(CommandConfiguration config, ITranslationLookup layout, WhitelistManager whitelistManager) : base(config,
        layout)
    {
        _whitelistManager = whitelistManager;
        Name = "bhwhitelist";
        Description = "Whitelists a global banned player";
        Alias = "bhwl";
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
        var result = await _whitelistManager.ActionWhitelist(gameEvent.Target);
        gameEvent.Origin.Tell($"{(result ? "Whitelisted" : "Removed from Whitelist")}: {gameEvent.Target.CleanedName}");
    }
}
