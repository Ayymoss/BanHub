using BanHub.Domain.Entities;

namespace BanHub.Domain.Interfaces.Repositories;

public interface IAliasRepository
{
    Task<EFAlias?> GetAliasByUserNameAndIpAddressAsync(string notificationPlayerAliasUserName, string notificationPlayerAliasIpAddress, CancellationToken cancellationToken);
    Task AddAliasAsync(EFAlias alias);
}
