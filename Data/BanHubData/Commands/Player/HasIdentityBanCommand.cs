using MediatR;

namespace BanHubData.Commands.Player;

public class HasIdentityBanCommand : IRequest<bool>
{
    public string Identity { get; set; }
}
