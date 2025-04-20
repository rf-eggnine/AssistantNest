// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AssistantNest.Extensions;

internal static class WebApplicationExtensions
{
    const string ApiKeyHeaderName = "X-Api-Key";
    const string TimeStampHeaderName = "X-Time-Stamp";
    private static readonly string ApiKeySecret = Environment.GetEnvironmentVariable("ANAPI_API_KEY") ?? throw new InvalidOperationException("ANAPI_API_KEY not set in environment variables");

    static bool ValidateAuth(string apiKey, out IResult result, params string[] dateString)
    {
        result = Results.Unauthorized();
        if (!apiKey.Equals(ApiKeySecret))
        {
            return false;
        }
        result = Results.Ok();
        return true;
    }

    internal static WebApplication UseMyCookiePolicy(this WebApplication app)
    {
        app.UseCookiePolicy(new CookiePolicyOptions()
            {
                CheckConsentNeeded = context => 
                {
                    if(bool.TryParse(context.Request.Headers[Constants.HeaderAcceptedCookies], out bool headerAcceptedCookies)
                        && headerAcceptedCookies)
                    {
                        app.Logger.LogDebug("header acceptedCookies true");
                        return false;
                    }
                    if(bool.TryParse(context.Request.Query[Constants.QueryStringKeyAcceptedCookies], out bool acceptCookies)
                        && acceptCookies)
                    {
                        app.Logger.LogDebug("query string acceptedCookies true");
                        return false;
                    }
                    if(context.Request.Cookies.ContainsKey(Constants.UserSessionCookieKey))
                    {
                        app.Logger.LogDebug("cookie already set");
                        return false;
                    }
                    app.Logger.LogDebug("cookes not accepted");
                    return true;
                },
                ConsentCookieValue = $"UserAcceptedCookiesOn{DateTime.UtcNow}",
                OnAppendCookie = context => 
                {
                    if(context.HasConsent)
                    {
                        context.IssueCookie = true;
                    }
                    else
                    {
                        app.Logger.LogDebug("Append cookie denied");
                    }
                },
                OnDeleteCookie = context =>
                {
                    if(context.HasConsent)
                    {
                        context.IssueCookie = true;
                    }
                    else
                    {
                        app.Logger.LogDebug("Delete cookie denied");
                    }
                },
            });
        return app;
    }
}
