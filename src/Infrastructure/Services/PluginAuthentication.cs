using System.Net;
using BanHub.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BanHub.Infrastructure.Services;

[AttributeUsage(AttributeTargets.Method)]
public class PluginAuthentication : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext? filterContext)
    {
        if (filterContext?.HttpContext.RequestServices.GetService(typeof(IPluginAuthenticationCache))
            is not IPluginAuthenticationCache apiKey) return;

        filterContext.HttpContext.Request.Headers.TryGetValue("BanHubApiToken", out var authTokens);
        var token = authTokens.FirstOrDefault();

        if (token is null)
        {
            SetUnauthorizedResponse(filterContext, HttpStatusCode.ExpectationFailed, "Authenticated endpoint. Provide token");
            return;
        }

        if (IsValidToken(token, apiKey))
        {
            SetAuthorizedResponse(filterContext, token);
            return;
        }

        SetUnauthorizedResponse(filterContext, HttpStatusCode.Forbidden, "Unauthorized");
    }

    private static void SetAuthorizedResponse(ActionContext context, string token)
    {
        context.HttpContext.Response.Headers.Add("BanHubApiToken", token);
        context.HttpContext.Response.Headers.Add("AuthStatus", "Authorized");
    }

    private static void SetUnauthorizedResponse(AuthorizationFilterContext context, HttpStatusCode statusCode, string reason)
    {
        context.HttpContext.Response.StatusCode = (int)statusCode;
        var responseFeature = context.HttpContext.Features.Get<IHttpResponseFeature>();
        if (responseFeature is not null) responseFeature.ReasonPhrase = reason;
        context.Result = new JsonResult(reason)
        {
            Value = new
            {
                Status = "Error",
                Message = reason
            }
        };
    }

    private static bool IsValidToken(string authToken, IPluginAuthenticationCache pluginAuthenticationCache) =>
        Guid.TryParse(authToken, out var key) && pluginAuthenticationCache.ExistsApiKey(key);
}
