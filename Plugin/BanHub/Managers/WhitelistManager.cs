using Data.Models.Client;
using Microsoft.Extensions.Logging;
using SharedLibraryCore.Interfaces;

namespace BanHub.Managers;

public class WhitelistManager
{
    private readonly IMetaServiceV2 _metaService;
    private readonly ILogger<WhitelistManager> _logger;

    public WhitelistManager(IMetaServiceV2 metaService, ILogger<WhitelistManager> logger)
    {
        _metaService = metaService;
        _logger = logger;
    }

    public async Task<bool> ActionWhitelist(EFClient client)
    {
        if (await IsWhitelisted(client))
        {
            await _metaService.RemovePersistentMeta("BHWhitelist", client.ClientId);
            _logger.LogInformation("{Client} has been removed from the whitelist", client.ClientId);
            return false;
        }

        await _metaService.SetPersistentMeta("BHWhitelist", "true", client.ClientId);
        _logger.LogInformation("{Client} has been added to the whitelist", client.ClientId);
        return true;
    }

    public async Task<bool> IsWhitelisted(EFClient client)
    {
        var entity = await _metaService.GetPersistentMeta("BHWhitelist", client.ClientId);
        return entity is not null;
    }
}
