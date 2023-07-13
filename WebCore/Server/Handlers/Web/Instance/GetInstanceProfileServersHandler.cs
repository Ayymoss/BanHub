using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Instance;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Instance;

public class GetInstanceProfileServersHandler : IRequestHandler<GetInstanceProfileServersCommand,
    IEnumerable<BanHub.WebCore.Shared.Models.InstanceProfileView.Server>>
{
    private readonly DataContext _context;

    public GetInstanceProfileServersHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BanHub.WebCore.Shared.Models.InstanceProfileView.Server>> Handle(GetInstanceProfileServersCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _context.Servers.Where(x => x.Instance.InstanceGuid == request.Identity)
            .Where(x => x.Updated < DateTimeOffset.UtcNow.AddMonths(1))
            .Select(x => new BanHub.WebCore.Shared.Models.InstanceProfileView.Server
            {
                ServerName = x.ServerName,
                ServerIp = x.ServerIp,
                ServerPort = x.ServerPort,
                ServerGame = x.ServerGame,
                Updated = x.Updated,
                ServerId = x.ServerId
            }).ToListAsync(cancellationToken);
        return result;
    }
}
