using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Domains;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Statistics;
using BanHubData.Enums;
using BanHubData.Mediatr.Commands.Requests.Server;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Plugin.Server;

public class CreateOrUpdateServerHandler(DataContext context, IPublisher publisher) 
    : IRequestHandler<CreateOrUpdateServerCommand, ControllerEnums.ReturnState>
{
    public async Task<ControllerEnums.ReturnState> Handle(CreateOrUpdateServerCommand request, CancellationToken cancellationToken)
    {
        var instance = await context.Communities
            .FirstOrDefaultAsync(x => x.CommunityGuid == request.CommunityGuid, cancellationToken: cancellationToken);
        if (instance is null) return ControllerEnums.ReturnState.NotFound;

        var server = await context.Servers
            .FirstOrDefaultAsync(x => x.ServerId == request.ServerId, cancellationToken: cancellationToken);
        if (server is not null)
        {
            server.Updated = DateTimeOffset.UtcNow;
            server.ServerName = request.ServerName;
            server.ServerGame = request.ServerGame;
            context.Servers.Update(server);
            await context.SaveChangesAsync(cancellationToken);
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

        await publisher.Publish(new UpdateStatisticsNotification
        {
            StatisticType = ControllerEnums.StatisticType.ServerCount,
            StatisticTypeAction = ControllerEnums.StatisticTypeAction.Add
        }, cancellationToken);

        context.Servers.Add(efServer);
        await context.SaveChangesAsync(cancellationToken);
        return ControllerEnums.ReturnState.Ok;
    }
}
