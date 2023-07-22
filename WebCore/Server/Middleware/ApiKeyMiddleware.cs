using System.Collections.Concurrent;
using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Middleware;

public class ApiKeyMiddleware : IMiddleware
{
    private readonly ApiKeyCache _apiKeyCache;
    private readonly DataContext _dataContext;

    public ApiKeyMiddleware(ApiKeyCache apiKeyCache, DataContext dataContext)
    {
        _apiKeyCache = apiKeyCache;
        _dataContext = dataContext;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_apiKeyCache.ApiKeys is null)
        {
            var keys = await _dataContext.Communities
                .Where(x => x.Active)
                .ToDictionaryAsync(x => x.CommunityGuid, x => x.ApiKey);

            _apiKeyCache.ApiKeys = new ConcurrentDictionary<Guid, Guid>(keys);
        }

        await next(context);
    }
}
