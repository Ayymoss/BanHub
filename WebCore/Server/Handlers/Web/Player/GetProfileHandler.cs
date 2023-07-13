using BanHub.WebCore.Shared.Commands.PlayerProfile;
using MediatR;

namespace BanHub.WebCore.Server.Handlers.Web.Player;

public class GetProfileHandler : IRequestHandler<GetProfileCommand, Shared.Models.PlayerProfileView.Player>
{
    public GetProfileHandler()
    {
        
    }
    public Task<Shared.Models.PlayerProfileView.Player> Handle(GetProfileCommand request, CancellationToken cancellationToken)
    {
        
    }
}
