using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Services;
using BanHubData.Commands.Instance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Plugin.Instance;

public class IsInstanceActiveHandler : IRequestHandler<IsInstanceActiveCommand, bool>
{
    private readonly ApiKeyCache _apiKeyCache;
    private readonly DataContext _context;

    public IsInstanceActiveHandler(ApiKeyCache apiKeyCache, DataContext context)
    {
        _apiKeyCache = apiKeyCache;
        _context = context;
    }

    public async Task<bool> Handle(IsInstanceActiveCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Instances.SingleOrDefaultAsync(x => x.InstanceGuid == request.InstanceGuid,
                cancellationToken: cancellationToken) is not { } result)
            return false;

        if (result.Active && _apiKeyCache.ApiKeys is not null && !_apiKeyCache.ApiKeys.Contains(result.ApiKey))
            _apiKeyCache.ApiKeys.Add(result.ApiKey);

        return result.Active;
    }
}
