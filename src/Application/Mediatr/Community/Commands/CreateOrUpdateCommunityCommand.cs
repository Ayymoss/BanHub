using BanHub.Domain.Enums;
using BanHub.Domain.ValueObjects.Plugin;
using MediatR;

namespace BanHub.Application.Mediatr.Community.Commands;

public class CreateOrUpdateCommunityCommand : CreateOrUpdateCommunitySlim, IRequest<ControllerEnums.ReturnState>
{
    public string? HeaderIp { get; set; }
}
