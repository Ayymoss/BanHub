﻿using BanHub.Application.DTOs.WebView.CommunitiesView;
using BanHub.Application.Mediatr.Community.Commands;
using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.ValueObjects.Services;
using BanHub.Infrastructure.Context;
using BanHub.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories.Pagination;

public class CommunitiesPaginationQueryHelper(DataContext context) : IResourceQueryHelper<GetCommunitiesPaginationCommand, Community>
{
    public async Task<PaginationContext<Community>> QueryResourceAsync(GetCommunitiesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = request.Privileged
            ? context.Communities.AsQueryable()
            : context.Communities.Where(x => x.Active).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
            query = query.Where(search =>
                EF.Functions.ILike(search.CommunityGuid.ToString(), $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.CommunityName, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.CommunityIp, $"%{request.SearchString}%"));

        if (request.Sorts.Any())
            query = request.Sorts.Aggregate(query, (current, sort) => sort.Property switch
            {
                "CommunityGuid" => current.ApplySort(sort, p => p.Id),
                "CommunityName" => current.ApplySort(sort, p => p.CommunityName),
                "CommunityWebsite" => current.ApplySort(sort, p => p.CommunityIpFriendly ?? p.CommunityIp),
                "HeartBeat" => current.ApplySort(sort, p => p.HeartBeat),
                "Created" => current.ApplySort(sort, p => p.Created),
                "ServerCount" => current.ApplySort(sort, p => p.ServerConnections.Count),
                _ => current
            });

        var count = await query.CountAsync(cancellationToken: cancellationToken);

        var pagedData = await query
            .Skip(request.Skip)
            .Take(request.Top)
            .Select(instance => new Community
            {
                Active = instance.Active,
                CommunityGuid = instance.CommunityGuid,
                CommunityWebsite = instance.CommunityIpFriendly ?? $"{instance.CommunityIp}:{instance.CommunityPort}",
                CommunityName = instance.CommunityName,
                HeartBeat = instance.HeartBeat,
                Created = instance.Created,
                ServerCount = instance.ServerConnections.Count
            }).ToListAsync(cancellationToken: cancellationToken);

        return new PaginationContext<Community>
        {
            Data = pagedData,
            Count = count
        };
    }
}
