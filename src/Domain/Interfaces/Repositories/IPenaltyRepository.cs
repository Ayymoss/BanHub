using BanHub.Domain.Entities;
using BanHub.Domain.ValueObjects.Repository.Penalty;

namespace BanHub.Domain.Interfaces.Repositories;

public interface IPenaltyRepository
{
    Task<int> GetPenaltyCountAsync(CancellationToken cancellationToken);
    Task<IEnumerable<LatestBans>> GetLatestBansAsync(CancellationToken cancellationToken);
    Task<int> GetCommunityPenaltiesCountAsync(Guid communityGuid, bool automated, CancellationToken cancellationToken);
    Task<int> AddOrUpdatePenaltyAsync(EFPenalty penalty, CancellationToken cancellationToken);
    Task<EFPenalty?> GetPenaltyAsync(Guid penaltyGuid, CancellationToken cancellationToken);
    Task RemovePenaltyAsync(EFPenalty penalty, CancellationToken cancellationToken);
    Task UpdatePenaltiesAsync(IEnumerable<EFPenalty> penalties, CancellationToken cancellationToken);
    Task<bool> HasActiveGlobalBanAsync(int targetId, CancellationToken cancellationToken);

    Task<IEnumerable<EFPenalty>> CurrentActivePenaltiesByCommunityAsync(Guid communityGuid, int targetId,
        CancellationToken cancellationToken);
}
