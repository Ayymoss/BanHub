using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models.Domains;
using BanHub.WebCore.Server.Notifications.Statistics;
using BanHubData.Commands.Community.Server;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Requests.Plugin.Community.Server;

public class CreateOrUpdateServerHandler : IRequestHandler<CreateOrUpdateServerCommand, ControllerEnums.ReturnState>
{
    private readonly DataContext _context;
    private readonly IMediator _mediator;

    public CreateOrUpdateServerHandler(DataContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<ControllerEnums.ReturnState> Handle(CreateOrUpdateServerCommand request, CancellationToken cancellationToken)
    {
        var instance = await _context.Communities
            .FirstOrDefaultAsync(x => x.CommunityGuid == request.CommunityGuid, cancellationToken: cancellationToken);
        if (instance is null) return ControllerEnums.ReturnState.NotFound;

        var server = await _context.Servers
            .FirstOrDefaultAsync(x => x.ServerId == request.ServerId, cancellationToken: cancellationToken);
        if (server is not null)
        {
            server.Updated = DateTimeOffset.UtcNow;
            server.ServerName = request.ServerName;
            server.ServerGame = request.ServerGame;
            _context.Servers.Update(server);
            await _context.SaveChangesAsync(cancellationToken);
            return ControllerEnums.ReturnState.NoContent;
        }

        var efServer = new EFServer
        {
            ServerId = request.ServerId,
            ServerName = request.ServerName,
            ServerIp = request.ServerIp,
            ServerPort = request.ServerPort,
            CommunityId = instance.Id,
            ServerGame = request.ServerGame,
            Updated = DateTimeOffset.UtcNow
        };

        await _mediator.Publish(new UpdateStatisticsNotification
        {
            StatisticType = ControllerEnums.StatisticType.ServerCount,
            StatisticTypeAction = ControllerEnums.StatisticTypeAction.Add
        }, cancellationToken);

        _context.Servers.Add(efServer);
        await _context.SaveChangesAsync(cancellationToken);
        return ControllerEnums.ReturnState.Ok;
    }
}
