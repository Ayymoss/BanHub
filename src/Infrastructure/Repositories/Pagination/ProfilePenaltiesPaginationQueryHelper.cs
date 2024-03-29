﻿using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.ValueObjects.Services;
using BanHub.Infrastructure.Context;
using BanHub.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories.Pagination;

public class ProfilePenaltiesPaginationQueryHelper(DataContext context) : IResourceQueryHelper<GetProfilePenaltiesPaginationCommand, Penalty>
{
    public async Task<PaginationContext<Penalty>> QueryResourceAsync(GetProfilePenaltiesPaginationCommand request, CancellationToken cancellationToken)
    {
         var query = context.Penalties
            .Where(x => x.Recipient.Identity == request.Identity)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
            query = query.Where(search =>
                EF.Functions.ILike(search.PenaltyGuid.ToString(), $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Reason, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Issuer.CurrentAlias.Alias.UserName, $"%{request.SearchString}%"));

        if (request.Sorts.Any())
            query = request.Sorts.Aggregate(query, (current, sort) => sort.Property switch
            {
                "IssuerUserName" => current.ApplySort(sort, p => p.Issuer.CurrentAlias.Alias.UserName),
                "RecipientUserName" => current.ApplySort(sort, p => p.Recipient.CurrentAlias.Alias.UserName),
                "Reason" => current.ApplySort(sort, p => p.Reason),
                "PenaltyType" => current.ApplySort(sort, p => p.PenaltyType),
                "PenaltyStatus" => current.ApplySort(sort, p => p.PenaltyStatus),
                "PenaltyScope" => current.ApplySort(sort, p => p.PenaltyScope),
                "CommunityName" => current.ApplySort(sort, p => p.Community.CommunityName),
                "Submitted" => current.ApplySort(sort, p => p.Submitted),
                _ => current
            });

        var count = await query.CountAsync(cancellationToken: cancellationToken);
        var pagedData = await query
            .Skip(request.Skip)
            .Take(request.Top)
            .Select(x => new Penalty
            {
                PenaltyGuid = x.PenaltyGuid,
                IssuerUserName = x.Issuer.CurrentAlias.Alias.UserName,
                IssuerIdentity = x.Issuer.Identity,
                Reason = !request.Privileged && x.Automated
                    ? "Automated Detection"
                    : x.Reason,
                Evidence = x.Evidence,
                CommunityName = x.Community.CommunityName,
                CommunityGuid = x.Community.CommunityGuid,
                Expiration = x.Expiration,
                PenaltyType = x.PenaltyType,
                PenaltyScope = x.PenaltyScope,
                PenaltyStatus = x.PenaltyStatus,
                Submitted = x.Submitted,
                Automated = x.Automated
            }).ToListAsync(cancellationToken: cancellationToken);

        return new PaginationContext<Penalty>
        {
            Data = pagedData,
            Count = count
        };
    }
}
