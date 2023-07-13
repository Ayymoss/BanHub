using System.Net;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BanHub.WebCore.Server.Services;

[AttributeUsage(AttributeTargets.Method)]
public class PluginAuthentication : Attribute, IAuthorizationFilter
{
    /// <summary>  
    /// Check if user can be authenticated
    /// </summary>  
    public void OnAuthorization(AuthorizationFilterContext? filterContext)
    {
        if (filterContext is null) return;

        filterContext.HttpContext.Request.Query.TryGetValue("authToken", out var authTokens);
        var token = authTokens.FirstOrDefault();

        if (filterContext.HttpContext.RequestServices.GetService(typeof(ApiKeyCache)) is not ApiKeyCache apiKey) return;

        if (token is not null)
        {
            if (IsValidToken(token, apiKey))
            {
                filterContext.HttpContext.Response.Headers.Add("authToken", token);
                filterContext.HttpContext.Response.Headers.Add("AuthStatus", "Authorized");
                filterContext.HttpContext.Response.Headers.Add("storeAccessiblity", "Authorized");
                return;
            }

            filterContext.HttpContext.Response.Headers.Add("authToken", token);
            filterContext.HttpContext.Response.Headers.Add("AuthStatus", "NotAuthorized");
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            filterContext.HttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Unauthorized";
            filterContext.Result = new JsonResult("Unauthorized")
            {
                Value = new
                {
                    Status = "Error",
                    Message = "Unauthorized"
                },
            };
        }
        else
        {
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
            filterContext.HttpContext.Response.HttpContext.Features
                .Get<IHttpResponseFeature>().ReasonPhrase = "Authenticated endpoint. Provide token";
            filterContext.Result = new JsonResult("Authenticated endpoint. Provide token")
            {
                Value = new
                {
                    Status = "Error",
                    Message = "Authenticated endpoint. Provide token"
                },
            };
        }
    }

    private static bool IsValidToken(string authToken, ApiKeyCache apiKeyCache)
    {
        if (!Guid.TryParse(authToken, out var guid)) return false;
        var exists = apiKeyCache.ApiKeys?.Contains(guid);
        return exists ?? false;
    }
}
