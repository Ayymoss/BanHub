using MediatR;

namespace Data.Commands.Player;

public class GetPlayerTokenCommand : IRequest<string?>
{
    public string Identity { get; set; }
}
