using System.Text.RegularExpressions;
using SharedLibraryCore;
using SharedLibraryCore.Commands;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Commands;

public class SubmitEvidenceCommand : Command
{
    public SubmitEvidenceCommand(CommandConfiguration config, ITranslationLookup layout) : base(config, layout)
    {
        Name = "submitevidence";
        Description = "Submit evidence for a players ban";
        Alias = "evidence";
        Permission = EFClient.Permission.SeniorAdmin;
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
            gameEvent.Origin.Tell(Plugin.Translations.NotActive);
            return;
        }

        const string regex = @"^([0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}) (((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube(-nocookie)?\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?)$";
        var match = Regex.Match(gameEvent.Data, regex);
        bool result;

        if (match.Success)
        {
            var guidCheck = Guid.TryParse(match.Groups[1].ToString(), out var guid);
            var evidence = match.Groups[3].ToString();

            if (guidCheck) result = await Plugin.EndpointManager.SubmitInformation(guid, evidence);
            else result = false;
        }
        else result = false;

        switch (result)
        {
            case true:
                gameEvent.Origin.Tell(Plugin.Translations.SubmitEvidenceSuccess);
                break;
            case false:
                gameEvent.Origin.Tell(Plugin.Translations.SubmitEvidenceFail);
                break;
        }
    }
}
