using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models.Domains;
using BanHubData.Commands.Player;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Plugin.Player;

public class CreateOrUpdatePlayerHandler : IRequestHandler<CreateOrUpdatePlayerCommand, string>
{
    private readonly DataContext _context;
    private readonly IStatisticService _statisticService;

    public CreateOrUpdatePlayerHandler(DataContext context, IStatisticService statisticService)
    {
        _context = context;
        _statisticService = statisticService;
    }

    public async Task<string> Handle(CreateOrUpdatePlayerCommand request, CancellationToken cancellationToken)
    {
         var user = await _context.Players.AsTracking()
            .Include(x => x.Aliases)
            .SingleOrDefaultAsync(user => user.Identity == request.PlayerIdentity, cancellationToken: cancellationToken);

        var efServer = await _context.Servers.AsNoTracking()
            .Where(c => c.ServerId == request.ServerId)
            .FirstOrDefaultAsync(x => x.Instance.InstanceGuid == request.InstanceGuid, cancellationToken: cancellationToken);

        var utcTimeNow = DateTimeOffset.UtcNow;

        // Update existing user
        if (user is not null)
        {
            var mostRecentAlias = await _context.CurrentAliases
                .AsTracking()
                .SingleOrDefaultAsync(x => x.PlayerId == user.Id, cancellationToken: cancellationToken);

            if (mostRecentAlias is not null && 
                (mostRecentAlias.Alias.UserName != request.PlayerAliasUserName || mostRecentAlias.Alias.IpAddress != request.PlayerAliasIpAddress))
            {
                var updatedAlias = new EFAlias
                {
                    UserName = request.PlayerAliasUserName,
                    IpAddress = request.PlayerAliasIpAddress,
                    Changed = utcTimeNow,
                    PlayerId = user.Id
                };

                user.Aliases.Add(updatedAlias);
                mostRecentAlias.Alias = updatedAlias;
                _context.CurrentAliases.Update(mostRecentAlias);
            }

            if (efServer is not null)
            {
                var server = new EFServerConnection
                {
                    Connected = utcTimeNow,
                    PlayerId = user.Id,
                    ServerId = efServer.Id,
                };
                _context.ServerConnections.Add(server);
            }

            user.HeartBeat = utcTimeNow;
            user.TotalConnections++;
            _context.Players.Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            return user.Identity;
        }

        // Create the user
        var entity = new EFPlayer
        {
            Identity = request.PlayerIdentity,
            HeartBeat = utcTimeNow,
            WebRole = WebRole.WebUser,
            InstanceRole = request.PlayerInstanceRole,
            Created = utcTimeNow,
            PlayTime = TimeSpan.Zero,
            TotalConnections = 1
        };

        var alias = new EFAlias
        {
            Player = entity,
            UserName = request.PlayerAliasUserName,
            IpAddress = request.PlayerAliasIpAddress,
            Changed = utcTimeNow
        };

        var currentAlias = new EFCurrentAlias
        {
            Alias = alias,
            Player = entity
        };

        if (efServer is not null)
        {
            var server = new EFServerConnection
            {
                Connected = utcTimeNow,
                Player = entity,
                ServerId = efServer.Id
            };
            _context.ServerConnections.Add(server);
        }

        await _statisticService.UpdateStatisticAsync(ControllerEnums.StatisticType.EntityCount,
            ControllerEnums.StatisticTypeAction.Add);

        entity.CurrentAlias = currentAlias;
        _context.CurrentAliases.Add(currentAlias);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Identity;
    }
}
