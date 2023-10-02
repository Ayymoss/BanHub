using BanHub.Domain.Entities;
using BanHub.Domain.ValueObjects.Repository.Player;

namespace BanHub.Domain.Interfaces.Repositories;

public interface IServerConnectionRepository
{
    Task<LastServerInfo?> GetLastConnectedServerAsync(string identity, CancellationToken cancellationToken);
    Task AddServerConnectionAsync(EFServerConnection serverConnection);
}
