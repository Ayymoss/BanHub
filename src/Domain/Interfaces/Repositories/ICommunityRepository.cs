using BanHub.Domain.Entities;
using BanHub.Domain.ValueObjects.Repository.Community;

namespace BanHub.Domain.Interfaces.Repositories;

public interface ICommunityRepository
{
    Task<EFCommunity?> GetCommunityAsync(Guid communityGuid, CancellationToken cancellationToken);
    Task<bool> IsCommunityApiKeyValidAsync(Guid communityGuid, Guid apiKey, CancellationToken cancellationToken);
    Task<int?> GetCommunityIdAsync(Guid communityGuid, CancellationToken cancellationToken);
    Task<int> GetCommunityCountAsync(CancellationToken cancellationToken);
    Task<bool> IsCommunityActiveAsync(Guid communityGuid, CancellationToken cancellationToken);
    Task<CommunityView?> GetCommunityProfileAsync(Guid communityGuid, CancellationToken cancellationToken);
    Task<int> AddOrUpdateCommunityAsync(EFCommunity community, CancellationToken cancellationToken);
}
