using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Domains;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Statistics;
using BanHub.WebCore.Server.Domains;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Statistics;
using BanHub.WebCore.Server.Utilities;
using BanHubData.Enums;
using BanHubData.Mediatr.Commands.Events.Player;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Events.Player;

public class CreateOrUpdatePlayerHandler : INotificationHandler<CreateOrUpdatePlayerNotification>
{
    private readonly DataContext _context;
    private readonly IMediator _mediator;

    public CreateOrUpdatePlayerHandler(DataContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task Handle(CreateOrUpdatePlayerNotification request, CancellationToken cancellationToken)
    {
        var user = await GetUserWithAliases(request.PlayerIdentity, cancellationToken);
        var efServer = await GetServer(request.ServerId, request.CommunityGuid, cancellationToken);
        var utcTimeNow = DateTimeOffset.UtcNow;

        if (user is not null)
        {
            await UpdateOrCreateAlias(user, request.PlayerAliasUserName, request.PlayerAliasIpAddress, utcTimeNow, cancellationToken);
            UpdateServerConnection(user, efServer, utcTimeNow);

            user.CommunityRole = request.PlayerCommunityRole;
            user.Heartbeat = utcTimeNow;
            user.TotalConnections++;
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        // Create the user
        var entity = CreateNewUser(request, utcTimeNow);
        var alias = CreateNewAlias(entity, request, utcTimeNow);
        var currentAlias = CreateNewCurrentAlias(entity, alias);
        UpdateServerConnection(entity, efServer, utcTimeNow);

        await _mediator.Publish(new UpdateStatisticsNotification
        {
            StatisticType = ControllerEnums.StatisticType.PlayerCount,
            StatisticTypeAction = ControllerEnums.StatisticTypeAction.Add
        }, cancellationToken);

        entity.CurrentAlias = currentAlias;
        _context.CurrentAliases.Add(currentAlias);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<EFPlayer?> GetUserWithAliases(string identity, CancellationToken cancellationToken)
    {
        return await _context.Players.AsTracking()
            .Include(x => x.Aliases)
            .Where(x => x.Identity == identity)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);
    }

    private async Task<EFServer?> GetServer(string? serverId, Guid communityGuid, CancellationToken cancellationToken)
    {
        return await _context.Servers.AsNoTracking()
            .Where(c => c.ServerId == serverId)
            .Where(x => x.Community.CommunityGuid == communityGuid)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    private async Task UpdateOrCreateAlias(EFPlayer user, string userName, string ipAddress, DateTimeOffset utcTimeNow,
        CancellationToken cancellationToken)
    {
        var mostRecentAlias = await _context.CurrentAliases
            .AsTracking()
            .Include(efCurrentAlias => efCurrentAlias.Alias)
            .SingleOrDefaultAsync(x => x.PlayerId == user.Id, cancellationToken: cancellationToken);

        var hasAliasChanged = mostRecentAlias?.Alias.UserName != userName
                              || mostRecentAlias.Alias.IpAddress != ipAddress;

        if (mostRecentAlias is not null && hasAliasChanged)
        {
            var updatedAlias = new EFAlias
            {
                UserName = userName.FilterUnknownCharacters(),
                IpAddress = ipAddress,
                Created = utcTimeNow,
                PlayerId = user.Id
            };

            user.Aliases.Add(updatedAlias);
            mostRecentAlias.Alias = updatedAlias;
            _context.CurrentAliases.Update(mostRecentAlias);
        }
    }

    private void UpdateServerConnection(EFPlayer user, EFServer? efServer, DateTimeOffset utcTimeNow)
    {
        if (efServer is null) return;
        var server = new EFServerConnection
        {
            Connected = utcTimeNow,
            PlayerId = user.Id,
            ServerId = efServer.Id,
        };
        _context.ServerConnections.Add(server);
    }

    private static EFPlayer CreateNewUser(CreateOrUpdatePlayerNotification request, DateTimeOffset utcTimeNow)
    {
        return new EFPlayer
        {
            Identity = request.PlayerIdentity,
            Heartbeat = utcTimeNow,
            WebRole = WebRole.User,
            CommunityRole = request.PlayerCommunityRole,
            Created = utcTimeNow,
            PlayTime = TimeSpan.Zero,
            TotalConnections = 1
        };
    }

    private static EFAlias CreateNewAlias(EFPlayer entity, CreateOrUpdatePlayerNotification request, DateTimeOffset utcTimeNow)
    {
        return new EFAlias
        {
            Player = entity,
            UserName = request.PlayerAliasUserName,
            IpAddress = request.PlayerAliasIpAddress,
            Created = utcTimeNow
        };
    }

    private static EFCurrentAlias CreateNewCurrentAlias(EFPlayer entity, EFAlias alias)
    {
        return new EFCurrentAlias
        {
            Alias = alias,
            Player = entity
        };
    }
}
