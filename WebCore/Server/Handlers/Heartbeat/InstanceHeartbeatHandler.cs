using BanHub.WebCore.Server.Context;
using Data.Commands.Heartbeat;
using Data.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Heartbeat;

public class InstanceHeartbeatHandler : IRequestHandler<InstanceHeartbeatCommand, ControllerEnums.ReturnState>
{
    private readonly DataContext _context;

    public InstanceHeartbeatHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<ControllerEnums.ReturnState> Handle(InstanceHeartbeatCommand request, CancellationToken cancellationToken)
    {
        var instance = await _context.Instances
            .AsTracking()
            .FirstOrDefaultAsync(x => x.InstanceGuid == request.InstanceGuid && x.ApiKey == request.ApiKey,
                cancellationToken: cancellationToken);
        if (instance is null) return ControllerEnums.ReturnState.NotFound;

        instance.HeartBeat = DateTimeOffset.UtcNow;
        _context.Instances.Update(instance);
        await _context.SaveChangesAsync(cancellationToken);
        return instance.Active ? ControllerEnums.ReturnState.Ok : ControllerEnums.ReturnState.Accepted;
    }
}
