using Data.Models.Client;
using SharedLibraryCore.Interfaces;

namespace BanHub.Managers;

public class WhitelistManager
{
    private readonly IMetaServiceV2 _metaService;

    public WhitelistManager(IMetaServiceV2 metaService)
    {
        _metaService = metaService;
    }

    public async Task<bool> ActionWhitelist(EFClient client)
    {
        if (await IsWhitelisted(client))
        {
            await RemoveWhitelist(client);
            return false;
        }

        await AddWhitelist(client);
        return true;
    }

    public async Task<bool> IsWhitelisted(EFClient client)
    {
        var entity = await _metaService.GetPersistentMeta("BHWhitelist", client.ClientId);
        return entity is not null;
    }

    private async Task AddWhitelist(EFClient client)
    {
        var entity = await _metaService.GetPersistentMeta("BHWhitelist", client.ClientId);
        if (entity is not null) return;
        await _metaService.SetPersistentMeta("BHWhitelist", "true", client.ClientId);
    }

    private async Task RemoveWhitelist(EFClient client)
    {
        var entity = await _metaService.GetPersistentMeta("BHWhitelist", client.ClientId);
        if (entity is null) return;
        await _metaService.RemovePersistentMeta("BHWhitelist", client.ClientId);
    }
}
