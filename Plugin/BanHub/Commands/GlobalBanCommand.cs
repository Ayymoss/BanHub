using BanHub.Configuration;
using BanHub.Managers;
using BanHubData.Enums;
using SharedLibraryCore;
using SharedLibraryCore.Commands;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace BanHub.Commands;

public class GlobalBanCommand : Command
{
    private readonly BanHubConfiguration _bhConfig;
    private readonly EndpointManager _endpointManager;

    public GlobalBanCommand(CommandConfiguration config, ITranslationLookup layout, BanHubConfiguration bhConfig,
        EndpointManager endpointManager) : base(config, layout)
    {
        _bhConfig = bhConfig;
        _endpointManager = endpointManager;
        Name = "bhglobalban";
        Description = "Bans a player from all servers";
        Alias = "gban";
        Permission = EFClient.Permission.SeniorAdmin;
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
        if (!Plugin.InstanceActive)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.NotActive);
            return;
        }

        if (gameEvent.Target.ClientId == 1)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.CannotTargetServer);
            return;
        }

        var result = await _endpointManager
            .NewPenalty(PenaltyType.Ban.ToString(), gameEvent.Origin, gameEvent.Target, gameEvent.Data, scope: PenaltyScope.Global);

        switch (result.Item1)
        {
            case true:
                gameEvent.Origin.Tell(_bhConfig.Translations.GlobalBanCommandSuccess.FormatExt(gameEvent.Target.CleanedName, gameEvent.Data, result.Item2));
                gameEvent.Origin.Tell(_bhConfig.Translations.GlobalBanCommandSuccessFollow);
                gameEvent.Target.Ban("^1Globally banned!^7\nBanHub.gg", SharedLibraryCore.Utilities.IW4MAdminClient(gameEvent.Target.CurrentServer), false);
                break;
            case false:
                gameEvent.Origin.Tell(_bhConfig.Translations.GlobalBanCommandFail);
                break;
        }
    }
}
