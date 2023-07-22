using System.Collections.Concurrent;

namespace BanHub.WebCore.Server.Services;

public class ApiKeyCache
{
    private ConcurrentDictionary<Guid, Guid>? _apiKeys;

    /// <summary>
    /// Checks if the API key cache is empty.
    /// </summary>
    public bool IsEmpty => _apiKeys?.IsEmpty ?? true;

    /// <summary>
    /// Adds or updates the API key for the community.
    /// </summary>
    /// <param name="communityGuid"></param>
    /// <param name="apiKey"></param>
    public void TryAdd(Guid communityGuid, Guid apiKey) => _apiKeys?.AddOrUpdate(communityGuid, apiKey, (key, value) => apiKey);

    /// <summary>
    /// Removes the API key for the community.
    /// </summary>
    /// <param name="communityGuid"></param>
    public void TryRemove(Guid communityGuid) => _apiKeys?.TryRemove(communityGuid, out _);

    /// <summary>
    /// Checks if the API key exists in the cache.
    /// </summary>
    /// <param name="apiKey"></param>
    /// <returns></returns>
    public bool ExistsApiKey(Guid apiKey) => _apiKeys is not null && _apiKeys.Values.Contains(apiKey);

    /// <summary>
    /// Sets the API key concurrent dictionary.
    /// </summary>
    /// <param name="apiKeys"></param>
    public void SetApiKeys(ConcurrentDictionary<Guid, Guid> apiKeys) => _apiKeys = apiKeys;
}
