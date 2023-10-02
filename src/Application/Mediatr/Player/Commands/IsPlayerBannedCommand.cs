using BanHub.Domain.ValueObjects.Plugin;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class IsPlayerBannedCommand : IsPlayerBannedCommandSlim, IRequest<bool>
{
}
