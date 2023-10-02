using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class GetProfileCommand : IRequest<DTOs.WebView.PlayerProfileView.Player?>
{
    public string Identity { get; set; }
    public bool Privileged { get; set; }
}
