using MediatR;

namespace BanHubData.Mediatr.Commands.Requests.Player;

public class IsPlayerBannedCommand : IRequest<bool>
{
    public string Identity { get; set; }
    public string IpAddress { get; set; }
}
