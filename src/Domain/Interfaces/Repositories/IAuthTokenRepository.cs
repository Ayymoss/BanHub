using BanHub.Domain.Entities;

namespace BanHub.Domain.Interfaces.Repositories;

public interface IAuthTokenRepository
{
    Task<EFAuthToken?> GetActiveTokenByPlayerIdAsync(int playerId, CancellationToken cancellationToken);
    Task<EFAuthToken?> GetActiveTokenByTokenAsync(string token, CancellationToken cancellationToken);
    Task ExpireAuthTokenAsync(EFAuthToken authToken, CancellationToken cancellationToken);
    Task CreateNewAuthTokenAsync(EFAuthToken authToken, CancellationToken cancellationToken);
}
