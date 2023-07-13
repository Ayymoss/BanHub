using MediatR;

namespace BanHubData.Commands.Player;

public class GetPlayerTokenCommand : IRequest<string?>
{
    public string Identity { get; set; }
}
