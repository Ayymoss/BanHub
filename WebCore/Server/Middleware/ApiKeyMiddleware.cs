using System.Collections.Concurrent;
using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Middleware;

public class ApiKeyMiddleware : IMiddleware
{
    private readonly ApiKeyCache _apiKeyCache;
    private readonly DataContext _context;

    public ApiKeyMiddleware(ApiKeyCache apiKeyCache, DataContext context)
    {
        _apiKeyCache = apiKeyCache;
        _context = context;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_apiKeyCache.IsEmpty)
        {
            var keys = await _context.Communities
                .Where(x => x.Active)
                .ToDictionaryAsync(x => x.CommunityGuid, x => x.ApiKey);
            _apiKeyCache.SetApiKeys(new ConcurrentDictionary<Guid, Guid>(keys));
        }

        await next(context);
    }
}
