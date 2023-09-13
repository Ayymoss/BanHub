using BanHub.WebCore.Server.Utilities;

namespace BanHub.WebCore.Server.Middleware;

public class PluginVersionCheckMiddleware : IMiddleware
{
    private readonly Configuration _configuration;

    public PluginVersionCheckMiddleware(Configuration configuration)
    {
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Headers.TryGetValue("BanHubPluginVersion", out var pluginVersion))
        {
            var version = pluginVersion.FirstOrDefault();
            if (string.IsNullOrEmpty(version))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Plugin version header is missing");
                return;
            }

            try
            {
                var incomingVersion = new Version(version);
                if (incomingVersion < _configuration.PluginVersion)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Plugin version is out of date");
                    return;
                }
            }
            catch (ArgumentException)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid Plugin version format");
                return;
            }
        }

        await next(context);
    }
}
