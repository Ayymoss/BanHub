using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileCommand, WebUser?>
{
    private readonly ISignedInUsersManager _signedInUsersManager;

    public GetUserProfileHandler(ISignedInUsersManager signedInUsersManager)
    {
        _signedInUsersManager = signedInUsersManager;
    }

    public Task<WebUser?> Handle(GetUserProfileCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_signedInUsersManager.GetSignedInUser(request.SignedInGuid));
    }
        
}
