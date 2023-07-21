using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Player;

public class GetProfileConnectionsHandler : IRequestHandler<GetProfileConnectionsCommand, IEnumerable<Connection>>
{
    private readonly DataContext _context;

    public GetProfileConnectionsHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Connection>> Handle(GetProfileConnectionsCommand request, CancellationToken cancellationToken)
    {
        var result = await _context.ServerConnections
            .Where(x => x.Player.Identity == request.Identity)
            .Select(x => new Connection
            {
                ServerName = x.Server.ServerName,
                ServerGame = x.Server.ServerGame,
                Connected = x.Connected,
                ServerPort = x.Server.ServerPort,
                ServerIp = x.Server.ServerIp,
                CommunityIp = x.Server.Community.CommunityIp,
                CommunityName = x.Server.Community.CommunityName
            }).ToListAsync(cancellationToken);
        return result;
    }
}
