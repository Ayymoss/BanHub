using System.Security.Claims;
using BanHubData.Enums;
using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests;

public class WebTokenLoginCommand : IRequest<WebTokenLoginCommandResponse>
{
    public string Token { get; set; }
}

public class WebTokenLoginCommandResponse
{
    public ControllerEnums.ReturnState ReturnState { get; set; }
    public ClaimsIdentity? ClaimsIdentity { get; set; }
}
