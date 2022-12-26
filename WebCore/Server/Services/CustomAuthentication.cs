using System.Net;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GlobalInfraction.WebCore.Server.Services;

[AttributeUsage(AttributeTargets.Method)]
public class CustomAuthorization : Attribute, IAuthorizationFilter
{
    /// <summary>  
    /// This will Authorize User  
    /// </summary>  
    public void OnAuthorization(AuthorizationFilterContext? filterContext)
    {
        if (filterContext == null) return;

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
            filterContext.HttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Not Authorized";
            filterContext.Result = new JsonResult("NotAuthorized")
            {
                Value = new
                {
                    Status = "Error",
                    Message = "Invalid Token"
                },
            };
        }
        else
        {
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
            filterContext.HttpContext.Response.HttpContext.Features
                .Get<IHttpResponseFeature>().ReasonPhrase = "Please Provide authToken";
            filterContext.Result = new JsonResult("Please Provide authToken")
            {
                Value = new
                {
                    Status = "Error",
                    Message = "Please Provide authToken"
                },
            };
        }
    }

    public bool IsValidToken(string authToken, ApiKeyCache apiKeyCache)
    {
        if (!Guid.TryParse(authToken, out var guid)) return false;
        var exists = apiKeyCache.ApiKeys?.Contains(guid);
        return exists ?? false;
    }
}
