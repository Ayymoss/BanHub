using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Penalty;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Penalty;

public class GetInstancePenaltiesHandler : IRequestHandler<GetInstancePenaltiesCommand,
    IEnumerable<Shared.Models.InstanceProfileView.Penalty>>
{
    private readonly DataContext _context;

    public GetInstancePenaltiesHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Shared.Models.InstanceProfileView.Penalty>> Handle(GetInstancePenaltiesCommand request,
        CancellationToken cancellationToken)
    {
        var results = await _context.Penalties
            .Where(x => x.Instance.InstanceGuid == request.Identity)
            .Select(x => new Shared.Models.InstanceProfileView.Penalty
            {
                PenaltyGuid = x.PenaltyGuid,
                TargetIdentity = x.Target.Identity,
                AdminIdentity = x.Admin.Identity,
                AdminUserName = x.Admin.CurrentAlias.Alias.UserName,
                Reason = request.Privileged && x.Automated
                    ? x.Reason
                    : x.Automated
                        ? "Automated Detection"
                        : x.Reason,
                Evidence = x.Evidence,
                TargetUserName = x.Target.CurrentAlias.Alias.UserName,
                InstanceGuid = x.Instance.InstanceGuid,
                Expiration = x.Expiration,
                PenaltyType = x.PenaltyType,
                PenaltyScope = x.PenaltyScope,
                PenaltyStatus = x.PenaltyStatus,
                Submitted = x.Submitted
            }).ToListAsync(cancellationToken: cancellationToken);
        return results;
    }
}
