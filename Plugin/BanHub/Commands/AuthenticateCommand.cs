using SharedLibraryCore;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace BanHub.Commands;

public class AuthenticateCommand : Command
{
    public AuthenticateCommand(CommandConfiguration config, ITranslationLookup layout) : base(config, layout)
    {
        Name = "banhubauth";
        Description = "Get an authentication code";
        Alias = "bhauth";
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
