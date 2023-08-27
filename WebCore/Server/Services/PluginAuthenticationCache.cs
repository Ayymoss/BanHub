using System.Collections.Concurrent;
using BanHub.WebCore.Server.Interfaces;

namespace BanHub.WebCore.Server.Services;

public class PluginAuthenticationCache : IPluginAuthenticationCache
{
    private ConcurrentDictionary<Guid, Guid>? _apiKeys;

    public bool IsEmpty => _apiKeys?.IsEmpty ?? true;
    public void TryAdd(Guid communityGuid, Guid apiKey) => _apiKeys?.AddOrUpdate(communityGuid, apiKey, (key, value) => apiKey);
    public void TryRemove(Guid communityGuid) => _apiKeys?.TryRemove(communityGuid, out _);
    public bool ExistsApiKey(Guid apiKey) => _apiKeys is not null && _apiKeys.Values.Contains(apiKey);
    public void SetApiKeys(ConcurrentDictionary<Guid, Guid> apiKeys) => _apiKeys = apiKeys;
}
