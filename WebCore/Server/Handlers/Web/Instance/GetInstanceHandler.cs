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
        var guidParse = Guid.TryParse(request.InstanceGuid, out var guidResult);
        if (!guidParse) return null;

        var result = await _context.Instances
            .Where(x => x.InstanceGuid == guidResult)
            .Select(x => new Shared.Models.InstanceProfileView.Instance
            {
                InstanceGuid = x.InstanceGuid,
                InstanceIp = x.InstanceIp,
                InstanceName = x.InstanceName,
                About = x.About,
                Socials = x.Socials,
                Active = x.Active,
                HeartBeat = x.HeartBeat,
                Created = x.Created
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return result;
    }
}
