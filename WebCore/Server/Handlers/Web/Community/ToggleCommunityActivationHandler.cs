using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Commands.Community;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Community;

public class ToggleCommunityActivationHandler : IRequestHandler<ToggleCommunityActivationCommand, bool>
{
    private readonly ApiKeyCache _apiKeyCache;
    private readonly DataContext _context;

    public ToggleCommunityActivationHandler(ApiKeyCache apiKeyCache, DataContext context)
    {
        _apiKeyCache = apiKeyCache;
        _context = context;
    }

    public async Task<bool> Handle(ToggleCommunityActivationCommand request, CancellationToken cancellationToken)
    {
        var instance = await _context.Communities
            .FirstOrDefaultAsync(x => x.CommunityGuid == request.CommunityGuid, cancellationToken: cancellationToken);

        if (instance is null || _apiKeyCache.IsEmpty) return false;
        instance.Active = !instance.Active;

        // TODO: Check this
        if (instance.Active) _apiKeyCache.TryRemove(instance.CommunityGuid);
        else _apiKeyCache.TryAdd(instance.CommunityGuid, instance.ApiKey);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
