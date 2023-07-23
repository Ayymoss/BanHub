using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Commands;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileCommand, WebUser?>
{
    private readonly DataContext _context;
    private readonly SignedInUsers _signedInUsers;

    public GetUserProfileHandler(DataContext context, SignedInUsers signedInUsers)
    {
        _context = context;
        _signedInUsers = signedInUsers;
    }

    public Task<WebUser?> Handle(GetUserProfileCommand request, CancellationToken cancellationToken) =>
        Task.FromResult(_signedInUsers.GetSignedInUser(request.SignedInGuid));
}
