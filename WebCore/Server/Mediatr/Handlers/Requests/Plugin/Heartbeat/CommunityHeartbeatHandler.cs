﻿using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Services;
using BanHubData.Enums;
using BanHubData.Mediatr.Commands.Requests.Heartbeat;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Plugin.Heartbeat;

public class CommunityHeartbeatHandler : IRequestHandler<CommunityHeartbeatCommand, SignalREnums.ReturnState>
{
    private readonly DataContext _context;

    public CommunityHeartbeatHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<SignalREnums.ReturnState> Handle(CommunityHeartbeatCommand request, CancellationToken cancellationToken)
    {
        var instance = await _context.Communities
            .AsTracking()
            .FirstOrDefaultAsync(x => x.CommunityGuid == request.CommunityGuid && x.ApiKey == request.ApiKey,
                cancellationToken: cancellationToken);
        if (instance is null) return SignalREnums.ReturnState.NotFound;

        instance.HeartBeat = DateTimeOffset.UtcNow;
        _context.Communities.Update(instance);
        await _context.SaveChangesAsync(cancellationToken);
        return SignalREnums.ReturnState.Ok;
    }
}