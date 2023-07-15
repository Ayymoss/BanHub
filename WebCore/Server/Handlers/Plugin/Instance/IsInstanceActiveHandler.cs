using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Services;
using BanHubData.Commands.Instance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Plugin.Instance;

public class IsInstanceActiveHandler : IRequestHandler<IsInstanceActiveCommand, bool>
{
    private readonly DataContext _context;

    public IsInstanceActiveHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(IsInstanceActiveCommand request, CancellationToken cancellationToken)
    {
        var result = await _context.Instances
            .SingleOrDefaultAsync(x => x.InstanceGuid == request.InstanceGuid, 
                cancellationToken: cancellationToken);
        
        return await _context.Instances
            .SingleOrDefaultAsync(x => x.InstanceGuid == request.InstanceGuid, 
                cancellationToken: cancellationToken) is {Active: true};
    }
}
