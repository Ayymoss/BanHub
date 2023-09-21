﻿using System.Collections.Concurrent;
using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Middleware;

public class ApiKeyMiddleware(IPluginAuthenticationCache pluginAuthenticationCache, DataContext dataContext)
    : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (pluginAuthenticationCache.IsEmpty)
        {
            var keys = await dataContext.Communities
                .Where(x => x.Active)
                .ToDictionaryAsync(x => x.CommunityGuid, x => x.ApiKey);
            pluginAuthenticationCache.SetApiKeys(new ConcurrentDictionary<Guid, Guid>(keys));
        }

        await next(context);
    }
}
