using BanHub.Domain.Enums;
using BanHub.Domain.ValueObjects.Plugin;
using MediatR;

namespace BanHub.Application.Mediatr.Server.Commands;

public class CreateOrUpdateServerCommand : CreateOrUpdateServerCommandSlim, IRequest<ControllerEnums.ReturnState>
{
}
