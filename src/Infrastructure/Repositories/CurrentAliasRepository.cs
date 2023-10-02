using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories;

public class CurrentAliasRepository(DataContext context) : ICurrentAliasRepository
{
    public async Task<int?> AddOrUpdateCurrentAliasAsync(EFCurrentAlias currentAlias, CancellationToken cancellationToken)
    {
        var existingAlias = await context.CurrentAliases
            .Where(x => x.Id == currentAlias.Id)
            .AnyAsync(cancellationToken: cancellationToken);

        if (existingAlias) context.CurrentAliases.Update(currentAlias);
        else context.CurrentAliases.Add(currentAlias);
        
        await context.SaveChangesAsync(cancellationToken);
        return currentAlias.Id;
    }

    public async Task<EFCurrentAlias?> GetCurrentAliasByPlayerIdAsync(int playerId, CancellationToken cancellationToken)
    {
        return await context.CurrentAliases
            .FirstOrDefaultAsync(ca => ca.PlayerId == playerId, cancellationToken: cancellationToken);
    }
}
