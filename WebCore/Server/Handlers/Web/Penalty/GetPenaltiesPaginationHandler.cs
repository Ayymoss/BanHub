using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Penalty;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BanHub.WebCore.Server.Handlers.Web.Penalty;

public class GetPenaltiesPaginationHandler : IRequestHandler<GetPenaltiesPaginationCommand,
    IEnumerable<Shared.Models.PenaltiesView.Penalty>>
{
    private readonly DataContext _context;

    public GetPenaltiesPaginationHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Shared.Models.PenaltiesView.Penalty>> Handle(GetPenaltiesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = _context.Penalties.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            query = query.Where(search =>
                EF.Functions.ILike(search.PenaltyGuid.ToString(), $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Admin.CurrentAlias.Alias.UserName, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Target.CurrentAlias.Alias.UserName, $"%{request.SearchString}%"));
        }

        query = request.SortLabel switch
        {
            "Id" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.PenaltyGuid),
            "Target Name" => query.OrderByDirection((SortDirection)request.SortDirection,
                key => key.Admin.CurrentAlias.Alias.UserName),
            "Admin Name" => query.OrderByDirection((SortDirection)request.SortDirection,
                key => key.Target.Penalties.Count),
            "Reason" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.Reason),
            "Type" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.PenaltyType),
            "Status" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.PenaltyStatus),
            "Scope" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.PenaltyScope),
            "Instance" => query.OrderByDirection((SortDirection)request.SortDirection,
                key => key.Instance.InstanceName),
            "Submitted" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.Submitted),
            _ => query
        };

        var pagedData = await query
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Where(x => x.PenaltyType == PenaltyType.Ban
                        || x.PenaltyType == PenaltyType.TempBan
                        || x.PenaltyType == PenaltyType.Kick
                        || x.PenaltyType == PenaltyType.Unban)
            .Select(penalty => new Shared.Models.PenaltiesView.Penalty
            {
                PenaltyGuid = penalty.PenaltyGuid,
                PenaltyType = penalty.PenaltyType,
                PenaltyStatus = penalty.PenaltyStatus,
                PenaltyScope = penalty.PenaltyScope,
                Submitted = penalty.Submitted,
                AdminIdentity = penalty.Admin.Identity,
                AdminUserName = penalty.Admin.CurrentAlias.Alias.UserName,
                Reason = request.Privileged && penalty.Automated
                    ? penalty.Reason
                    : penalty.Automated
                        ? "Automated Detection"
                        : penalty.Reason,
                Evidence = penalty.Evidence,
                Duration = penalty.Duration,
                InstanceGuid = penalty.Instance.InstanceGuid,
                TargetIdentity = penalty.Target.Identity,
                TargetUserName = penalty.Target.CurrentAlias.Alias.UserName,
                InstanceName = penalty.Instance.InstanceName
            }).ToListAsync(cancellationToken: cancellationToken);

        return pagedData;
    }
}
