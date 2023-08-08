using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Services;
using BanHubData.Commands.Heartbeat;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Plugin.Heartbeat;

public class CommunityHeartbeatHandler : IRequestHandler<CommunityHeartbeatCommand, ControllerEnums.ReturnState>
{
    private readonly DataContext _context;
    private readonly Configuration _config;

    public CommunityHeartbeatHandler(DataContext context, Configuration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<ControllerEnums.ReturnState> Handle(CommunityHeartbeatCommand request, CancellationToken cancellationToken)
    {
        if (request.PluginVersion < _config.PluginVersion) return ControllerEnums.ReturnState.BadRequest;

        var instance = await _context.Communities
            .AsTracking()
            .FirstOrDefaultAsync(x => x.CommunityGuid == request.CommunityGuid && x.ApiKey == request.ApiKey,
                cancellationToken: cancellationToken);
        if (instance is null) return ControllerEnums.ReturnState.NotFound;

        instance.HeartBeat = DateTimeOffset.UtcNow;
        _context.Communities.Update(instance);
        await _context.SaveChangesAsync(cancellationToken);
        return instance.Active ? ControllerEnums.ReturnState.Ok : ControllerEnums.ReturnState.Accepted;
    }
}
