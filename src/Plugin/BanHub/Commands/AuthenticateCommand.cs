using BanHub.Plugin.Configuration;
using BanHub.Plugin.Managers;
using BanHub.Plugin.Models;
using SharedLibraryCore;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace BanHub.Plugin.Commands;

public class AuthenticateCommand : Command
{
    private readonly BanHubConfiguration _bhConfig;
    private readonly EndpointManager _endpointManager;
    private readonly CommunitySlim _communitySlim;

    public AuthenticateCommand(CommandConfiguration config, ITranslationLookup layout, BanHubConfiguration bhConfig,
        EndpointManager endpointManager, CommunitySlim communitySlim) : base(config, layout)
    {
        _bhConfig = bhConfig;
        _endpointManager = endpointManager;
        _communitySlim = communitySlim;
        Name = "bhauthenticate";
        Description = "Get an authentication code";
        Alias = "bhauth";
        Permission = EFClient.Permission.Moderator;
        RequiresTarget = false;
    }

    public override async Task ExecuteAsync(GameEvent gameEvent)
    {
        if (!_communitySlim.Active)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.NotActive.FormatExt(_bhConfig.Translations.BanHubName));
            return;
        }

        if (gameEvent.Origin.ClientId is 1)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.CannotAuthIW4MAdmin.FormatExt(_bhConfig.Translations.BanHubName));
            return;
        }

        var token = await _endpointManager.GetTokenAsync(gameEvent.Origin);
        if (token is null)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.TokenGenerationFailed.FormatExt(_bhConfig.Translations.BanHubName));
            return;
        }
        gameEvent.Origin.Tell(_bhConfig.Translations.ProvideToken.FormatExt(_bhConfig.Translations.BanHubName, token));
    }
}
