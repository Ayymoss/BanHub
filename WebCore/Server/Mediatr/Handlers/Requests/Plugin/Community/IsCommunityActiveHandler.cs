using BanHub.WebCore.Server.Context;
using BanHubData.Mediatr.Commands.Requests.Community;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Plugin.Community;

public class IsCommunityActiveHandler(DataContext context) : IRequestHandler<IsCommunityActiveCommand, bool>
{
    public async Task<bool> Handle(IsCommunityActiveCommand request, CancellationToken cancellationToken)
    {
        return await context.Communities
            .SingleOrDefaultAsync(x => x.CommunityGuid == request.CommunityGuid, 
                cancellationToken: cancellationToken) is {Active: true};
    }
}
