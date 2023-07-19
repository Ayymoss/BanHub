using BanHub.Configuration;
using BanHub.Managers;
using SharedLibraryCore;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace BanHub.Commands;

public class AuthenticateCommand : Command
{
    private readonly BanHubConfiguration _bhConfig;
    private readonly EndpointManager _endpointManager;

    public AuthenticateCommand(CommandConfiguration config, ITranslationLookup layout, BanHubConfiguration bhConfig,
        EndpointManager endpointManager) : base(config, layout)
    {
        _bhConfig = bhConfig;
        _endpointManager = endpointManager;
        Name = "bhauthenticate";
        Description = "Get an authentication code";
        Alias = "bhauth";
        Permission = EFClient.Permission.Moderator;
        RequiresTarget = false;
    }

    public override async Task ExecuteAsync(GameEvent gameEvent)
    {
        if (!Plugin.InstanceActive)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.NotActive);
            return;
        }

        if (gameEvent.Origin.ClientId is 1)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.CannotAuthIW4MAdmin);
            return;
        }

        var token = await _endpointManager.GetTokenAsync(gameEvent.Origin);
        if (token is null)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.TokenGenerationFailed);
            return;
        }

        gameEvent.Origin.Tell(_bhConfig.Translations.ProvideToken.FormatExt(token));
    }
}
