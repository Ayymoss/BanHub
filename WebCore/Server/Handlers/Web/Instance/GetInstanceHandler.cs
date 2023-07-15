using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Instance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Instance;

public class GetInstanceHandler : IRequestHandler<GetInstanceCommand, BanHub.WebCore.Shared.Models.InstanceProfileView.Instance?>
{
    private readonly DataContext _context;

    public GetInstanceHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Shared.Models.InstanceProfileView.Instance?> Handle(GetInstanceCommand request, CancellationToken cancellationToken)
    {
        var serverCount = await _context.Servers
            .CountAsync(x => x.Instance.InstanceGuid == request.InstanceGuid, cancellationToken: cancellationToken);

        var result = await _context.Instances
            .Where(x => x.InstanceGuid == request.InstanceGuid)
            .Select(x => new Shared.Models.InstanceProfileView.Instance
            {
                InstanceGuid = x.InstanceGuid,
                InstanceIp = x.InstanceIp,
                InstanceName = x.InstanceName,
                About = x.About,
                Socials = x.Socials,
                Active = x.Active,
                HeartBeat = x.HeartBeat,
                Created = x.Created,
                ServerCount = serverCount,
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return result;
    }
}
