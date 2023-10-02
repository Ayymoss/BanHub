using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories;

public class AliasRepository(DataContext context) : IAliasRepository
{
    public async Task AddAliasAsync(EFAlias alias)
    {
        context.Aliases.Add(alias);
        await context.SaveChangesAsync();
    }

    public async Task<EFAlias?> GetAliasByUserNameAndIpAddressAsync(string userName, string ipAddress, CancellationToken cancellationToken)
    {
        return await context.Aliases
            .Where(x => x.IpAddress == ipAddress)
            .Where(a => a.UserName == userName)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}
