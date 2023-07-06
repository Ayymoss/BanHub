using MediatR;

namespace Data.Commands.Player;

public class IsPlayerBannedCommand : IRequest<bool>
{
    public string Identity { get; set; }
}
