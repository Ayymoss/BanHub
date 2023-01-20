using GlobalInfractions.Enums;
using SharedLibraryCore;
using SharedLibraryCore.Commands;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Commands;

public class RevokeGlobalBanCommand : Command
{
    public RevokeGlobalBanCommand(CommandConfiguration config, ITranslationLookup layout) : base(config, layout)
    {
        Name = "revokeglobalban";
        Description = "Revokes a global ban from a player";
        Alias = "rgban";
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
            .RevokeGlobalBan(InfractionType.Ban, gameEvent.Origin, gameEvent.Target, gameEvent.Data, scope: InfractionScope.Global);
        // FIX GLOBAL BAN 3/4 PROBLEM

        switch (result.Item1)
        {
            case true:
                gameEvent.Origin.Tell(Plugin.Translations.GlobalBanCommandSuccess.FormatExt(gameEvent.Target.CleanedName, gameEvent.Data, result.Item2));
                gameEvent.Origin.Tell(Plugin.Translations.GlobalBanCommandSuccessFollow);
                gameEvent.Target.Unban("^1Globally banned!^7\nGlobalInfractions.com", Utilities.IW4MAdminClient(gameEvent.Target.CurrentServer));
                break;
            case false:
                gameEvent.Origin.Tell(Plugin.Translations.GlobalBanCommandFail);
                break;
        }
    }
}
