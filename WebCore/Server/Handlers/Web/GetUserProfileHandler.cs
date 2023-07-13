using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands;
using BanHub.WebCore.Shared.Commands.Web;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileCommand, WebUser?>
{
    private readonly DataContext _context;

    public GetUserProfileHandler(DataContext context)
    {
        _context = context;
    }
    public async Task<WebUser?> Handle(GetUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Players
            .Where(x => x.Id == request.UserId)
            .Select(f => new WebUser
            {
                UserName = f.CurrentAlias.Alias.UserName,
                WebRole = f.WebRole.ToString(),
                InstanceRole = f.InstanceRole.ToString(),
                Identity = f.Identity
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return user;
    }
}
