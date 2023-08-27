using System.Collections.Concurrent;

namespace BanHub.WebCore.Server.Interfaces;

public interface IPluginAuthenticationCache
{
    /// <summary>
    /// Checks if the API key cache is empty.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Adds or updates the API key for the community.
    /// </summary>
    /// <param name="communityGuid"></param>
    /// <param name="apiKey"></param>
    void TryAdd(Guid communityGuid, Guid apiKey);

    /// <summary>
    /// Removes the API key for the community.
    /// </summary>
    /// <param name="communityGuid"></param>
    void TryRemove(Guid communityGuid);

    /// <summary>
    /// Checks if the API key exists in the cache.
    /// </summary>
    /// <param name="apiKey"></param>
    /// <returns></returns>
    bool ExistsApiKey(Guid apiKey);

    /// <summary>
    /// Sets the API key concurrent dictionary.
    /// </summary>
    /// <param name="apiKeys"></param>
    void SetApiKeys(ConcurrentDictionary<Guid, Guid> apiKeys);
}
