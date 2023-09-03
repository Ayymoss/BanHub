﻿using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Servers;
using BanHub.WebCore.Shared.Models.Shared;
using BanHub.WebCore.Shared.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Server;

public class GetServersPaginationHandler : IRequestHandler<GetServersPaginationCommand, PaginationContext<Shared.Models.ServersView.Server>>
{
    private readonly DataContext _context;
    private readonly IMediator _mediator;

    public GetServersPaginationHandler(DataContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<PaginationContext<Shared.Models.ServersView.Server>> Handle(GetServersPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = await _context.Servers
            .Where(x => x.Updated < DateTimeOffset.UtcNow.AddHours(1))
            .Include(efServer => efServer.Community)
            .ToListAsync(cancellationToken: cancellationToken);

        var serverIds = query.Select(p => p.ServerIp).ToArray();
        var onlineCounts = await _mediator.Send(new GetServerOnlineCountsCommand {ServerIds = serverIds}, cancellationToken);

        var serverViews = query.Select(x => new Shared.Models.ServersView.Server
        {
            ServerIp = x.ServerIp.IsInternal() ? x.Community.CommunityIp : x.ServerIp,
            ServerPort = x.ServerPort,
            ServerGame = x.ServerGame,
            ServerName = x.ServerName,
            Updated = x.Updated,
            CommunityGuid = x.Community.CommunityGuid,
            CommunityName = x.Community.CommunityName,
            MaxPlayers = x.MaxPlayers,
            OnlineCount = onlineCounts.TryGetValue(x.ServerIp, out var count) ? count : 0
        }).ToList();
        if (!string.IsNullOrWhiteSpace(request.SearchString))
            serverViews = serverViews
                .Where(s => s.ServerName.Contains(request.SearchString) 
                            || s.ServerIp.Contains(request.SearchString) 
                            || s.ServerGame.ToString().Contains(request.SearchString)
                            || s.CommunityName.Contains(request.SearchString))
                .ToList();

        if (request.Sorts.Any())
            serverViews = request.Sorts.Aggregate(serverViews, (current, sort) => sort.Property switch
            {
                "OnlineCount" => current.OrderBy(s => s.OnlineCount).ToList(),
                "CommunityName" => current.OrderBy(s => s.CommunityName).ToList(),
                "ServerName" => current.OrderBy(s => s.ServerName).ToList(),
                "ServerIp" => current.OrderBy(s => s.ServerIp).ToList(),
                "ServerGame" => current.OrderBy(s => s.ServerGame).ToList(),
                "Updated" => current.OrderBy(s => s.Updated).ToList(),
                _ => current
            });

        var count = serverViews.Count;
        var pagedData = serverViews
            .Skip(request.Skip)
            .Take(request.Top)
            .ToList();

        return new PaginationContext<Shared.Models.ServersView.Server>
        {
            Data = pagedData,
            Count = count
        };
    }
}