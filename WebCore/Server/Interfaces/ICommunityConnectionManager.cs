namespace BanHub.WebCore.Server.Interfaces;

public interface ICommunityConnectionManager
{
    void AddOrUpdate(Guid communityId, string connectionId);
    bool TryGetConnectionId(Guid communityId, out string? connectionId);
}
