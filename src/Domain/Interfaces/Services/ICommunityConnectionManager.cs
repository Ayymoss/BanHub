namespace BanHub.Domain.Interfaces.Services;

public interface ICommunityConnectionManager
{
    void AddOrUpdate(Guid communityId, string connectionId);
    bool TryGetConnectionId(Guid communityId, out string? connectionId);
}
