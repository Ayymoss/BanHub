using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.ValueObjects.Repository.Server;
using BanHub.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories;

public class ServerRepository(DataContext context) : IServerRepository
{
    public async Task<IEnumerable<ServerIdentifier>> GetServerIdentifiersAsync(IEnumerable<string> serverIds,
        CancellationToken cancellationToken)
    {
        var servers = await context.Servers
            .Where(x => serverIds.Contains(x.ServerId))
            .Select(x => new ServerIdentifier
            {
                Id = x.Id,
                ServerId = x.ServerId
            }).ToListAsync(cancellationToken: cancellationToken);
        return servers;
    }

    public async Task<int> GetServerCountAsync(CancellationToken cancellationToken)
    {
        return await context.Servers.CountAsync(cancellationToken: cancellationToken);
    }

    public async Task<int> GetServerCountAsync(Guid communityGuid, CancellationToken cancellationToken)
    {
        return await context.Servers
            .Where(x => x.Community.CommunityGuid == communityGuid)
            .CountAsync(cancellationToken: cancellationToken);
    }

    public async Task<(List<ServerPaginationView> ServerPaginationView, string[] ServerIds)> GetServerPaginationAsync(
        CancellationToken cancellationToken)
    {
        // TODO: Can't move this to IResourceQueryHelper because return types are different.
        // Maybe revise? 

        var query = await context.Servers
            .Where(x => x.Updated < DateTimeOffset.UtcNow.AddHours(1))
            .Include(efServer => efServer.Community)
            .ToListAsync(cancellationToken: cancellationToken);
        var serverIds = query.Select(p => p.ServerId).ToArray();

        var queryModel = query.Select(x => new ServerPaginationView
        {
            ServerIp = x.ServerIp,
            CommunityIp = x.Community.CommunityIp,
            ServerPort = x.ServerPort,
            ServerGame = x.ServerGame,
            ServerName = x.ServerName,
            Updated = x.Updated,
            CommunityGuid = x.Community.CommunityGuid,
            CommunityName = x.Community.CommunityName,
            MaxPlayers = x.MaxPlayers,
        }).ToList();

        return (queryModel, serverIds);
    }

    public async Task<EFServer?> GetServerAsync(string serverId, CancellationToken cancellationToken)
    {
        var currentSentiment = await context.Servers
            .Where(x => x.ServerId == serverId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return currentSentiment;
    }

    public async Task<int?> AddOrUpdateServerAsync(EFServer server, CancellationToken cancellationToken)
    {
        var efServer = await context.Servers
            .Where(x => x.Id == server.Id)
            .AnyAsync(cancellationToken: cancellationToken);        
        
        if (efServer) context.Servers.Update(server);
        else context.Servers.Add(server);
        
        
        await context.SaveChangesAsync(cancellationToken);
        return server.Id;
    }
}
