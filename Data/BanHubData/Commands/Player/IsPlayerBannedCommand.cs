using MediatR;

namespace BanHubData.Commands.Player;

public class IsPlayerBannedCommand : IRequest<bool>
{
    public string Identity { get; set; }
    public string IpAddress { get; set; }
}
