using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories;

public class AuthTokenRepository(DataContext context) : IAuthTokenRepository
{
    public async Task<EFAuthToken?> GetActiveTokenByPlayerIdAsync(int playerId, CancellationToken cancellationToken)
    {
        return await GetActiveTokenQuery()
            .Where(x => x.PlayerId == playerId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EFAuthToken?> GetActiveTokenByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await GetActiveTokenQuery()
            .Where(x => x.Token == token)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task CreateNewAuthTokenAsync(EFAuthToken authToken, CancellationToken cancellationToken)
    {
        context.AuthTokens.Add(authToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExpireAuthTokenAsync(EFAuthToken authToken, CancellationToken cancellationToken)
    {
        authToken.Used = true;
        context.AuthTokens.Update(authToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    #region Private Methods

    private IQueryable<EFAuthToken> GetActiveTokenQuery()
    {
        return context.AuthTokens
            .Where(x => x.Expiration > DateTimeOffset.UtcNow)
            .Where(x => !x.Used);
    }

    #endregion
}
