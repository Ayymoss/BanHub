using MediatR;

namespace BanHubData.Mediatr.Commands.Requests.Player;

public class HasIdentityBanCommand : IRequest<bool>
{
    public string Identity { get; set; }
}
