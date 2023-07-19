using BanHub.WebCore.Shared.Models.PlayerProfileView;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.PlayerProfile;

public class GetProfileCommand : IRequest<Player?>
{
    public string Identity { get; set; }
    public bool Privileged { get; set; }
}
