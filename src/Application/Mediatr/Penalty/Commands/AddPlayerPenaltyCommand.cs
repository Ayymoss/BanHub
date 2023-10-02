using BanHub.Domain.Enums;
using BanHub.Domain.ValueObjects.Plugin;
using MediatR;

namespace BanHub.Application.Mediatr.Penalty.Commands;

public class AddPlayerPenaltyCommand : AddPlayerPenaltyCommandSlim, IRequest<(ControllerEnums.ReturnState, Guid?)>
{
}
