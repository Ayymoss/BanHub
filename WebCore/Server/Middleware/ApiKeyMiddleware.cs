using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Middleware;

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
        _apiKeyCache.ApiKeys ??= await _dataContext.Instances
            .Where(x => x.Active)
            .Select(x => x.ApiKey).ToListAsync();

        await next(context);
    }
}
