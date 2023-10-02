using BanHub.Domain.Enums;
using BanHub.Plugin.Configuration;
using BanHub.Plugin.Managers;
using BanHub.Plugin.Models;
using SharedLibraryCore;
using SharedLibraryCore.Commands;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace BanHub.Plugin.Commands;

public class GlobalBanCommand : Command
{
    private readonly BanHubConfiguration _bhConfig;
    private readonly EndpointManager _endpointManager;
    private readonly CommunitySlim _communitySlim;

    public GlobalBanCommand(CommandConfiguration config, ITranslationLookup layout, BanHubConfiguration bhConfig,
        EndpointManager endpointManager, CommunitySlim communitySlim) : base(config, layout)
    {
        _bhConfig = bhConfig;
        _endpointManager = endpointManager;
        _communitySlim = communitySlim;
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
        if (!_communitySlim.Active)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.NotActive.FormatExt(_bhConfig.Translations.BanHubName));
            return;
        }

        if (gameEvent.Target.ClientId is 1)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.CannotTargetServer.FormatExt(_bhConfig.Translations.BanHubName));
            return;
        }

        var result = await _endpointManager
            .AddPlayerPenaltyAsync(PenaltyType.Ban.ToString(), gameEvent.Origin, gameEvent.Target, gameEvent.Data,
                scope: PenaltyScope.Global);

        if (!result.Item1)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.GlobalBanCommandFail.FormatExt(_bhConfig.Translations.BanHubName));
            return;
        }
        
        gameEvent.Origin.Tell(_bhConfig.Translations.GlobalBanCommandSuccess
            .FormatExt(_bhConfig.Translations.BanHubName, gameEvent.Target.CleanedName, gameEvent.Data, result.Item2));
        gameEvent.Origin.Tell(_bhConfig.Translations.GlobalBanCommandSuccessFollow.FormatExt(_bhConfig.Translations.BanHubName, SubmitEvidenceCommand.CommandAlias));
        gameEvent.Target.SetAdditionalProperty("BanHubGlobalBan", true);
        gameEvent.Target.Ban("^1Globally banned!^7\nBanHub.gg",
            SharedLibraryCore.Utilities.IW4MAdminClient(gameEvent.Target.CurrentServer), false);
    }
}
