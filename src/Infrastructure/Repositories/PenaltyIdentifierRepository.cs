using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories;

public class PenaltyIdentifierRepository(DataContext context) : IPenaltyIdentifierRepository
{
    public async Task<bool> HasIdentityBanAsync(string identity, CancellationToken cancellationToken)
    {
        var hasIdentityBan = await context.PenaltyIdentifiers
            .Where(x => x.Expiration > DateTimeOffset.UtcNow)
            .Where(x => x.Identity == identity)
            .AnyAsync(cancellationToken: cancellationToken);

        return hasIdentityBan;
    }

    public async Task RemovePenaltyIdentifiersAsync(EFPenaltyIdentifier[] removeIdentifiers, CancellationToken cancellationToken)
    {
        context.PenaltyIdentifiers.RemoveRange(removeIdentifiers);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<EFPenaltyIdentifier>> GetActivePenaltyIdentifiersAsync(string ipAddress, string identity,
        CancellationToken cancellationToken)
    {
        var result = await context.PenaltyIdentifiers
            .Where(x => x.Expiration > DateTimeOffset.UtcNow)
            .Where(x => x.IpAddress == ipAddress || x.Identity == identity)
            .ToListAsync(cancellationToken: cancellationToken);
        return result;
    }

    public async Task AddPenaltyIdentifierAsync(EFPenaltyIdentifier penaltyIdentifier, CancellationToken cancellationToken)
    {
        context.PenaltyIdentifiers.Add(penaltyIdentifier);
        await context.SaveChangesAsync(cancellationToken);
    }
}
