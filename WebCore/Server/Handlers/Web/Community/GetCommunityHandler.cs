﻿using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Community;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Community;

public class GetCommunityHandler : IRequestHandler<GetCommunityCommand, Shared.Models.CommunityProfileView.Community?>
{
    private readonly DataContext _context;

    public GetCommunityHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Shared.Models.CommunityProfileView.Community?> Handle(GetCommunityCommand request, CancellationToken cancellationToken)
    {
        var serverCount = await _context.Servers
            .CountAsync(x => x.Community.CommunityGuid == request.CommunityGuid, cancellationToken: cancellationToken);

        var result = await _context.Community
            .Where(x => x.CommunityGuid == request.CommunityGuid)
            .Select(x => new Shared.Models.CommunityProfileView.Community
            {
                CommunityGuid = x.CommunityGuid,
                CommunityWebsite = x.CommunityIpFriendly ?? $"{x.CommunityIp}:{x.CommunityPort}",
                CommunityPort = x.CommunityPort,
                CommunityName = x.CommunityName,
                About = x.About,
                Socials = x.Socials,
                Active = x.Active,
                Connected = x.HeartBeat + TimeSpan.FromMinutes(5) > DateTimeOffset.UtcNow,
                Created = x.Created,
                ServerCount = serverCount,
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return result;
    }
}