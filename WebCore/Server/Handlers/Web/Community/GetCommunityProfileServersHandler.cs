using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Community;
using BanHub.WebCore.Shared.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Community;

public class GetCommunityProfileServersHandler : IRequestHandler<GetCommunityProfileServersCommand,
    IEnumerable<Shared.Models.CommunityProfileView.Server>>
{
    private readonly DataContext _context;

    public GetCommunityProfileServersHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Shared.Models.CommunityProfileView.Server>> Handle(GetCommunityProfileServersCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _context.Servers.Where(x => x.Community.CommunityGuid == request.Identity)
            .Where(x => x.Updated < DateTimeOffset.UtcNow.AddMonths(1))
            .Select(x => new Shared.Models.CommunityProfileView.Server
            {
                ServerName = x.ServerName,
                ServerIp = x.ServerIp.IsInternal() ? "?" : x.ServerIp, // TODO: Test this.
                ServerPort = x.ServerPort,
                ServerGame = x.ServerGame,
                Updated = x.Updated,
                ServerId = x.ServerId
            }).ToListAsync(cancellationToken);
        return result;
    }
}
