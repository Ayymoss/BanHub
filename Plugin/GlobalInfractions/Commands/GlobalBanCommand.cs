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
        if (!Plugin.FeaturesEnabled) return; // TODO: Remove tag
        
        if (!Plugin.InstanceActive)
        {
            gameEvent.Origin.Tell(Plugin.Translations.NotActive);
            return;
        }

        var result = await Plugin.EndpointManager
            .NewInfraction(InfractionType.Ban, gameEvent.Origin, gameEvent.Target, gameEvent.Data, scope: InfractionScope.Global);

        switch (result)
        {
            case true:
                gameEvent.Origin.Tell(Plugin.Translations.GlobalBanCommandSuccess.FormatExt(gameEvent.Target.CleanedName, gameEvent.Data));
                break;
            case false:
                gameEvent.Origin.Tell(Plugin.Translations.GlobalBanCommandFail);
                break;
        }
    }
}
