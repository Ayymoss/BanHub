using BanHub.Domain.Interfaces.Services;
using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class GetUserProfileHandler(ISignedInUsersManager signedInUsersManager) : IRequestHandler<GetUserProfileCommand, WebUser?>
{
    public Task<WebUser?> Handle(GetUserProfileCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(signedInUsersManager.GetSignedInUser(request.SignedInGuid));
    }
        
}
