using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web;

public class GetUserProfileHandler(ISignedInUsersManager signedInUsersManager) : IRequestHandler<GetUserProfileCommand, WebUser?>
{
    public Task<WebUser?> Handle(GetUserProfileCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(signedInUsersManager.GetSignedInUser(request.SignedInGuid));
    }
        
}
