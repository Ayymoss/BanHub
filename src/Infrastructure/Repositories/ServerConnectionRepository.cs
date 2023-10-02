using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.ValueObjects.Repository.Player;
using BanHub.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories;

public class ServerConnectionRepository(DataContext context) : IServerConnectionRepository
{
    public async Task<LastServerInfo?> GetLastConnectedServerAsync(string identity, CancellationToken cancellationToken)
    {
        var lastServer = await context.ServerConnections
            .Where(x => x.Player.Identity == identity)
            .OrderByDescending(x => x.Connected)
            .Select(x => new LastServerInfo
            {
                ServerName = x.Server.ServerName,
                CommunityName = x.Server.Community.CommunityName
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return lastServer;
    }

    public async Task AddServerConnectionAsync(EFServerConnection serverConnection)
    {
        context.ServerConnections.Add(serverConnection);
        await context.SaveChangesAsync();
    }

    
}
