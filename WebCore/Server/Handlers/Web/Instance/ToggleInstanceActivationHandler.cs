using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Commands.Instance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Instance;

public class ToggleInstanceActivationHandler : IRequestHandler<ToggleInstanceActivationCommand, bool>
{
    private readonly ApiKeyCache _apiKeyCache;
    private readonly DataContext _context;

    public ToggleInstanceActivationHandler(ApiKeyCache apiKeyCache, DataContext context)
    {
        _apiKeyCache = apiKeyCache;
        _context = context;
    }

    public async Task<bool> Handle(ToggleInstanceActivationCommand request, CancellationToken cancellationToken)
    {
        var instance = await _context.Instances
            .FirstOrDefaultAsync(x => x.InstanceGuid == request.InstanceGuid, cancellationToken: cancellationToken);

        if (instance is null || _apiKeyCache.ApiKeys is null) return false;
        instance.Active = !instance.Active;

        switch (instance.Active)
        {
            case true:
                _apiKeyCache.ApiKeys.TryRemove(instance.InstanceGuid, out _);
                break;
            case false:
                _apiKeyCache.ApiKeys.TryAdd(instance.InstanceGuid, instance.ApiKey);
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
