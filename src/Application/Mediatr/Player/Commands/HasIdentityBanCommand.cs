using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class HasIdentityBanCommand : IRequest<bool>
{
    public string Identity { get; set; }
}
