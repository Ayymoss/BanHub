using BanHub.Domain.Entities;
using BanHub.Domain.ValueObjects.Repository.Server;

namespace BanHub.Domain.Interfaces.Repositories;

public interface IServerRepository
{
    Task<IEnumerable<ServerIdentifier>> GetServerIdentifiersAsync(IEnumerable<string> serverIds, CancellationToken cancellationToken);
    Task<int> GetServerCountAsync(CancellationToken cancellationToken);
    Task<int> GetServerCountAsync(Guid communityGuid, CancellationToken cancellationToken);
    Task<EFServer?> GetServerAsync(string serverId, CancellationToken cancellationToken);
    Task<int?> AddOrUpdateServerAsync(EFServer server, CancellationToken cancellationToken);

    Task<(List<ServerPaginationView> ServerPaginationView, string[] ServerIds)> GetServerPaginationAsync(
        CancellationToken cancellationToken);
}
