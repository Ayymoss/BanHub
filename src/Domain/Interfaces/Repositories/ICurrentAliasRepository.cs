using BanHub.Domain.Entities;

namespace BanHub.Domain.Interfaces.Repositories;

public interface ICurrentAliasRepository
{
    Task<EFCurrentAlias?> GetCurrentAliasByPlayerIdAsync(int playerId, CancellationToken cancellationToken);
    Task<int?> AddOrUpdateCurrentAliasAsync(EFCurrentAlias currentAlias, CancellationToken cancellationToken);
}
