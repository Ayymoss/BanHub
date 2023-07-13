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
                InstanceIp = x.Server.Instance.InstanceIp,
                InstanceName = x.Server.Instance.InstanceName
            }).ToListAsync(cancellationToken);
        return result;
    }
}
