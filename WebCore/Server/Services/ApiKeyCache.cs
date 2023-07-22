using System.Collections.Concurrent;
using BanHub.WebCore.Server.Context;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Services;

public class ApiKeyCache
{
    private readonly DataContext _context;

    private readonly ConcurrentDictionary<Guid, Guid> _apiKeys;

    public ApiKeyCache(DataContext context)
    {
        _context = context;
        _apiKeys = new ConcurrentDictionary<Guid, Guid>();
    }

    public bool IsEmpty => _apiKeys.IsEmpty;

    /// <summary>
    /// Adds or updates the API key for the community.
    /// </summary>
    /// <param name="communityGuid"></param>
    /// <param name="apiKey"></param>
    public void TryAdd(Guid communityGuid, Guid apiKey) => _apiKeys.AddOrUpdate(communityGuid, apiKey, (key, value) => apiKey);

    /// <summary>
    /// Removes the API key for the community.
    /// </summary>
    /// <param name="communityGuid"></param>
    public void TryRemove(Guid communityGuid) => _apiKeys.TryRemove(communityGuid, out _);

    /// <summary>
    /// Attempts to get the API key for the community.
    /// </summary>
    /// <param name="communityGuid"></param>
    /// <param name="apiKey"></param>
    /// <returns></returns>
    public bool TryGetCommunityApiKey(Guid communityGuid, out Guid apiKey) => _apiKeys.TryGetValue(communityGuid, out apiKey);

    /// <summary>
    /// Checks if the community exists in the cache.
    /// </summary>
    /// <param name="communityGuid"></param>
    /// <returns></returns>
    public bool ExistsCommunity(Guid communityGuid) => _apiKeys.ContainsKey(communityGuid);

    /// <summary>
    /// Checks if the API key exists in the cache.
    /// </summary>
    /// <param name="apiKey"></param>
    /// <returns></returns>
    public bool ExistsApiKey(Guid apiKey) => _apiKeys.Values.Contains(apiKey);

    /// <summary>
    /// Loads all the API keys from the database.
    /// </summary>
    public async Task LoadApiKeys()
    {
        var keys = await _context.Communities
            .Where(x => x.Active)
            .ToDictionaryAsync(x => x.CommunityGuid, x => x.ApiKey);

        _apiKeys.Clear();
        foreach (var (key, value) in keys)
        {
            _apiKeys.TryAdd(key, value);
        }
    }
}
