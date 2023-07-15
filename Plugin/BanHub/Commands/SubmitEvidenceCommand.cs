using System.Text.RegularExpressions;
using BanHub.Configuration;
using BanHub.Managers;
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
        if (!Plugin.InstanceActive)
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.NotActive);
            return;
        }

        const string youtubeRegex = @"^([0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}) (((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube(-nocookie)?\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?)$";
        const string videoId = @"v=([^&]+)";
        var regex = Regex.Match(gameEvent.Data, youtubeRegex);
        var videoIdMatch = Regex.Match(gameEvent.Data, videoId);
        bool result;

        if (regex.Success)
        {
            var guidCheck = Guid.TryParse(regex.Groups[1].ToString(), out var guid);
            var evidence = videoIdMatch.Groups[1].ToString();

            if (guidCheck) result = await _endpointManager.AddPlayerPenaltyEvidenceAsync(guid, evidence, gameEvent.Origin);
            else result = false;
        }
        else
        {
            gameEvent.Origin.Tell(_bhConfig.Translations.SubmitEvidenceRegexFail);
            result = false;
        }

        switch (result)
        {
            case true:
                gameEvent.Origin.Tell(_bhConfig.Translations.SubmitEvidenceSuccess);
                break;
            case false:
                gameEvent.Origin.Tell(_bhConfig.Translations.SubmitEvidenceFail);
                break;
        }
    }
}
