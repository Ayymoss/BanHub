using System.Collections.Concurrent;
using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Middleware;

public class ApiKeyMiddleware : IMiddleware
{
    private readonly ApiKeyCache _apiKeyCache;

    public ApiKeyMiddleware(ApiKeyCache apiKeyCache)
    {
        _apiKeyCache = apiKeyCache;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_apiKeyCache.IsEmpty) await _apiKeyCache.LoadApiKeys();

        await next(context);
    }
}
