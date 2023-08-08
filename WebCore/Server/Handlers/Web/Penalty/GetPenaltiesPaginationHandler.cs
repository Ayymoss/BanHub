using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Penalty;
using BanHub.WebCore.Shared.Models.PenaltiesView;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BanHub.WebCore.Server.Handlers.Web.Penalty;

public class GetPenaltiesPaginationHandler : IRequestHandler<GetPenaltiesPaginationCommand, PenaltyContext>
{
    private readonly DataContext _context;

    public GetPenaltiesPaginationHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<PenaltyContext> Handle(GetPenaltiesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = _context.Penalties
            .Where(x => x.PenaltyType == PenaltyType.Ban
                        || x.PenaltyType == PenaltyType.TempBan
                        || x.PenaltyType == PenaltyType.Kick
                        || x.PenaltyType == PenaltyType.Unban)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            query = query.Where(search =>
                EF.Functions.ILike(search.PenaltyGuid.ToString(), $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Reason, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Issuer.CurrentAlias.Alias.UserName, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Recipient.CurrentAlias.Alias.UserName, $"%{request.SearchString}%"));
        }

        query = request.SortLabel switch
        {
            "Id" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.PenaltyGuid),
            "IssuerName" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.Issuer.CurrentAlias.Alias.UserName),
            "RecipientName" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.Recipient.Penalties.Count),
            "Reason" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.Reason),
            "Type" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.PenaltyType),
            "Status" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.PenaltyStatus),
            "Scope" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.PenaltyScope),
            "Community" => query.OrderByDirection((SortDirection)request.SortDirection,
                key => key.Community.CommunityName),
            "Submitted" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.Submitted),
            _ => query
        };

        var count = await query.CountAsync(cancellationToken: cancellationToken);
        var pagedData = await query
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(penalty => new Shared.Models.PenaltiesView.Penalty
            {
                PenaltyGuid = penalty.PenaltyGuid,
                PenaltyType = penalty.PenaltyType,
                PenaltyStatus = penalty.PenaltyStatus,
                PenaltyScope = penalty.PenaltyScope,
                Submitted = penalty.Submitted,
                AdminIdentity = penalty.Issuer.Identity,
                AdminUserName = penalty.Issuer.CurrentAlias.Alias.UserName,
                Reason = request.Privileged && penalty.Automated
                    ? penalty.Reason
                    : penalty.Automated
                        ? "Automated Detection"
                        : penalty.Reason,
                Evidence = penalty.Evidence,
                Expiration = penalty.Expiration,
                CommunityGuid = penalty.Community.CommunityGuid,
                TargetIdentity = penalty.Recipient.Identity,
                TargetUserName = penalty.Recipient.CurrentAlias.Alias.UserName,
                CommunityName = penalty.Community.CommunityName,
                EvidenceMissing = string.IsNullOrWhiteSpace(penalty.Evidence) && penalty.PenaltyScope == PenaltyScope.Global && !penalty.Automated
            }).ToListAsync(cancellationToken: cancellationToken);

        return new PenaltyContext
        {
            Penalties = pagedData,
            Count = count
        };
    }
}
