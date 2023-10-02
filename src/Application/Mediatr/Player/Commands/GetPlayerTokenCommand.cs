using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class GetPlayerTokenCommand : IRequest<string?>
{
    public string Identity { get; set; }
}
