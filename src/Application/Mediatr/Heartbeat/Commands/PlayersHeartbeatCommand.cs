using BanHub.Domain.Enums;
using BanHub.Domain.ValueObjects.Plugin.SignalR;
using MediatR;

namespace BanHub.Application.Mediatr.Heartbeat.Commands;

public class PlayersHeartbeatCommand : PlayersHeartbeatCommandSlim, IRequest<SignalREnums.ReturnState>
{
}
