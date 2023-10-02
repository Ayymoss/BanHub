using BanHub.Plugin.Configuration;
using BanHub.Plugin.Managers;
using BanHub.Plugin.Models;
using BanHub.Plugin.Utilities;
using SharedLibraryCore;
using SharedLibraryCore.Commands;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace BanHub.Plugin.Commands;

public class SubmitEvidenceCommand : Command
{
    private readonly BanHubConfiguration _bhConfig;
    private readonly EndpointManager _endpointManager;
    private readonly CommunitySlim _communitySlim;
    public const string CommandAlias = "bhe";

    public SubmitEvidenceCommand(CommandConfiguration config, ITranslationLookup layout, BanHubConfiguration bhConfig,
        EndpointManager endpointManager, CommunitySlim communitySlim) : base(config, layout)
    {
        _bhConfig = bhConfig;
        _endpointManager = endpointManager;
        _communitySlim = communitySlim;
        Name = "bhevidence";
        Description = "Submit evidence for a players ban";
        Alias = CommandAlias;
        Permission = EFClient.Permission.Moderator;
        RequiresTarget = false;
        Arguments = new[]
        {
            new CommandArgument
            {
                Name = "ban guid",
                Required = true
            },
            new CommandArgument
            {
                Name = "evidence url",
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

        var args = gameEvent.Data.Split(" ");
        var guidCheck = Guid.TryParse(args.FirstOrDefault(), out var guid);
        var videoId = args.LastOrDefault()?.GetYouTubeVideoId();

        if (string.IsNullOrEmpty(videoId) || !guidCheck)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.SubmitEvidenceUrlFail.FormatExt(_bhConfig.Translations.BanHubName));
            return;
        }

        var result = await _endpointManager.AddPlayerPenaltyEvidenceAsync(guid, videoId, gameEvent.Origin);
        if (!result)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.SubmitEvidenceFail.FormatExt(_bhConfig.Translations.BanHubName));
            return;
        }

        gameEvent.Origin.Tell(_bhConfig.Translations.SubmitEvidenceSuccess.FormatExt(_bhConfig.Translations.BanHubName));
    }
}
