using MediatR;

namespace BanHubData.Mediatr.Commands.Requests.Player;

public class GetPlayerTokenCommand : IRequest<string?>
{
    public string Identity { get; set; }
}
