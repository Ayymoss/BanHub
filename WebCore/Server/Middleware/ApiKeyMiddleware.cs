using System.Collections.Concurrent;
using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Middleware;

public class ApiKeyMiddleware : IMiddleware
{
    private readonly IPluginAuthenticationCache _pluginAuthenticationCache;
    private readonly DataContext _context;

    public ApiKeyMiddleware(IPluginAuthenticationCache pluginAuthenticationCache, DataContext context)
    {
        _pluginAuthenticationCache = pluginAuthenticationCache;
        _context = context;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_pluginAuthenticationCache.IsEmpty)
        {
            var keys = await _context.Communities
                .Where(x => x.Active)
                .ToDictionaryAsync(x => x.CommunityGuid, x => x.ApiKey);
            _pluginAuthenticationCache.SetApiKeys(new ConcurrentDictionary<Guid, Guid>(keys));
        }

        await next(context);
    }
}
