using SharedLibraryCore;
using SharedLibraryCore.Commands;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Commands;

public class GlobalInfractionsAuthCommand : Command
{
    public GlobalInfractionsAuthCommand(CommandConfiguration config, ITranslationLookup layout) : base(config, layout)
    {
        Name = "globalauth";
        Description = "Get an authentication code";
        Alias = "gauth";
        Permission = EFClient.Permission.Moderator;
        RequiresTarget = false;
    }

    public override async Task ExecuteAsync(GameEvent gameEvent)
    {
        if (!Plugin.InstanceActive)
        {
            gameEvent.Origin.Tell(Plugin.Translations.NotActive);
            return;
        }

        if (gameEvent.Origin.ClientId == 1)
        {
            gameEvent.Origin.Tell(Plugin.Translations.CannotAuthIW4MAdmin);
            return;
        }

        var token = await Plugin.EndpointManager.GenerateToken(gameEvent.Origin);
        if (token is null)
        {
            gameEvent.Origin.Tell(Plugin.Translations.TokenGenerationFailed);
            return;
        }

        gameEvent.Origin.Tell(Plugin.Translations.ProvideToken.FormatExt(token));
    }
}
