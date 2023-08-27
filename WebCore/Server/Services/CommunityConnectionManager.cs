using System.Collections.Concurrent;
using BanHub.WebCore.Server.Interfaces;

namespace BanHub.WebCore.Server.Services;

public class CommunityConnectionManager : ICommunityConnectionManager
{
    private readonly ConcurrentDictionary<Guid, string> _communityConnections = new();

    public void AddOrUpdate(Guid communityId, string connectionId)
    {
        _communityConnections.AddOrUpdate(communityId, connectionId, (_, _) => connectionId);
    }

    public bool TryGetConnectionId(Guid communityId, out string? connectionId)
    {
        return _communityConnections.TryGetValue(communityId, out connectionId);
    }
}
