using System.Linq.Dynamic.Core;
using BanHub.Domain.Entities;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.ValueObjects.Repository.Penalty;
using BanHub.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories;

public class PenaltyRepository(DataContext context) : IPenaltyRepository
{
    public async Task<int> GetPenaltyCountAsync(CancellationToken cancellationToken)
    {
        return await context.Penalties.CountAsync(cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<LatestBans>> GetLatestBansAsync(CancellationToken cancellationToken)
    {
        var bans = await context.Penalties
            .Where(x => x.PenaltyType == PenaltyType.Ban)
            .Where(x => x.PenaltyStatus == PenaltyStatus.Active)
            .Where(x => x.PenaltyScope == PenaltyScope.Global)
            .Where(x => x.Submitted > DateTimeOffset.UtcNow.AddMonths(-1)) // Arbitrary time frame. We don't care about anything too old.
            .OrderByDescending(x => x.Id)
            .Take(3)
            .Select(penalty => new LatestBans
            {
                Submitted = penalty.Submitted,
                RecipientIdentity = penalty.Recipient.Identity,
                RecipientUserName = penalty.Recipient.CurrentAlias.Alias.UserName,
                CommunityGuid = penalty.Community.CommunityGuid,
                CommunityName = penalty.Community.CommunityName,
                Reason = penalty.Reason,
                Automated = penalty.Automated,
            }).ToListAsync(cancellationToken: cancellationToken);
        return bans;
    }

    public async Task<int> GetCommunityPenaltiesCountAsync(Guid communityGuid, bool automated, CancellationToken cancellationToken)
    {
        return await context.Penalties
            .Where(x => x.Community.CommunityGuid == communityGuid)
            .Where(x => x.Automated == automated)
            .CountAsync(cancellationToken);
    }

    public async Task<EFPenalty?> GetPenaltyAsync(Guid penaltyGuid, CancellationToken cancellationToken)
    {
        // TODO: These queries need to be updated at some point. Returning a DTO then explicitly updating the property.
        var penalty = await context.Penalties
            .Include(x => x.Recipient)
            .ThenInclude(x => x.CurrentAlias)
            .ThenInclude(x => x.Alias)
            .Include(x => x.Issuer)
            .ThenInclude(x => x.CurrentAlias)
            .ThenInclude(x => x.Alias)
            .Where(x => x.PenaltyGuid == penaltyGuid)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return penalty;
    }

    public async Task<int> AddOrUpdatePenaltyAsync(EFPenalty penalty, CancellationToken cancellationToken)
    {
        var efPenalty = await context.Penalties
            .Where(x => x.Id == penalty.Id)
            .AnyAsync(cancellationToken: cancellationToken);

        if (efPenalty) context.Penalties.Update(penalty);
        else context.Penalties.Add(penalty);

        await context.SaveChangesAsync(cancellationToken);
        return penalty.Id;
    }

    public async Task RemovePenaltyAsync(EFPenalty penalty, CancellationToken cancellationToken)
    {
        context.Penalties.Remove(penalty);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePenaltiesAsync(IEnumerable<EFPenalty> penalties, CancellationToken cancellationToken)
    {
        context.Penalties.AddRange(penalties);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasActiveGlobalBanAsync(int targetId, CancellationToken cancellationToken)
    {
        var hasExistingGlobalBan = await context.Penalties
            .Where(x => x.RecipientId == targetId)
            .Where(x => x.PenaltyStatus == PenaltyStatus.Active)
            .Where(x => x.PenaltyType == PenaltyType.Ban)
            .Where(x => x.PenaltyScope == PenaltyScope.Global)
            .AnyAsync(cancellationToken: cancellationToken);

        return hasExistingGlobalBan;
    }

    public async Task<IEnumerable<EFPenalty>> CurrentActivePenaltiesByCommunityAsync(Guid communityGuid, int targetId, CancellationToken cancellationToken)
    {
        var penalties = await context.Penalties
            .Include(x => x.Identifier)
            .Where(i => i.Community.CommunityGuid == communityGuid)
            .Where(t => t.RecipientId == targetId)
            .Where(p => p.PenaltyType == PenaltyType.Ban || p.PenaltyType == PenaltyType.TempBan)
            .Where(p => p.PenaltyStatus == PenaltyStatus.Active)
            .ToListAsync(cancellationToken: cancellationToken);
        return penalties;
    }
}
