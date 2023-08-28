using BanHub.WebCore.Server.Context;
using BanHubData.Commands.Community;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Requests.Plugin.Community;

public class IsCommunityActiveHandler : IRequestHandler<IsCommunityActiveCommand, bool>
{
    private readonly DataContext _context;

    public IsCommunityActiveHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(IsCommunityActiveCommand request, CancellationToken cancellationToken)
    {
        return await _context.Communities
            .SingleOrDefaultAsync(x => x.CommunityGuid == request.CommunityGuid, 
                cancellationToken: cancellationToken) is {Active: true};
    }
}
