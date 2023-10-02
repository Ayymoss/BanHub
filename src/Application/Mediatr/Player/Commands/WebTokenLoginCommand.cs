using System.Security.Claims;
using BanHub.Domain.Enums;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class WebTokenLoginCommand : IRequest<WebTokenLoginCommandResponse>
{
    public string Token { get; set; }
}

public class WebTokenLoginCommandResponse
{
    public ControllerEnums.ReturnState ReturnState { get; set; }
    public ClaimsIdentity? ClaimsIdentity { get; set; }
}
