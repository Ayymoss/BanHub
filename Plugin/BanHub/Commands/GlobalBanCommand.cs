using BanHub.Enums;
using SharedLibraryCore;
using SharedLibraryCore.Commands;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace BanHub.Commands;

public class GlobalBanCommand : Command
{
    public GlobalBanCommand(CommandConfiguration config, ITranslationLookup layout) : base(config, layout)
    {
        Name = "globalban";
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
            gameEvent.Origin.Tell(Plugin.Translations.NotActive);
            return;
        }

        if (gameEvent.Target.ClientId == 1)
        {
            gameEvent.Origin.Tell(Plugin.Translations.CannotTargetServer);
            return;
        }

        var result = await Plugin.EndpointManager
            .NewPenalty(PenaltyType.Ban, gameEvent.Origin, gameEvent.Target, gameEvent.Data, scope: PenaltyScope.Global);

        switch (result.Item1)
        {
            case true:
                gameEvent.Origin.Tell(Plugin.Translations.GlobalBanCommandSuccess.FormatExt(gameEvent.Target.CleanedName, gameEvent.Data, result.Item2));
                gameEvent.Origin.Tell(Plugin.Translations.GlobalBanCommandSuccessFollow);
                gameEvent.Target.Ban("^1Globally banned!^7\nBanHub.gg", Utilities.IW4MAdminClient(gameEvent.Target.CurrentServer), false);
                break;
            case false:
                gameEvent.Origin.Tell(Plugin.Translations.GlobalBanCommandFail);
                break;
        }
    }
}
