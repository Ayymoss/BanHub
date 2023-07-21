using System.Text.RegularExpressions;
using BanHub.Configuration;
using BanHub.Managers;
using BanHubData.Extension;
using SharedLibraryCore;
using SharedLibraryCore.Commands;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace BanHub.Commands;

public class SubmitEvidenceCommand : Command
{
    private readonly BanHubConfiguration _bhConfig;
    private readonly EndpointManager _endpointManager;

    public SubmitEvidenceCommand(CommandConfiguration config, ITranslationLookup layout, BanHubConfiguration bhConfig,
        EndpointManager endpointManager) : base(config, layout)
    {
        _bhConfig = bhConfig;
        _endpointManager = endpointManager;
        Name = "bhevidence";
        Description = "Submit evidence for a players ban";
        Alias = "bhe";
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
        if (!Plugin.CommunityActive)
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

        var result = await _endpointManager.AddPlayerPenaltyEvidenceAsync(guid, videoId, gameEvent.Origin, gameEvent.Target);
        if (!result)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.SubmitEvidenceFail.FormatExt(_bhConfig.Translations.BanHubName));
            return;
        }

        gameEvent.Origin.Tell(_bhConfig.Translations.SubmitEvidenceSuccess.FormatExt(_bhConfig.Translations.BanHubName));
    }
}
