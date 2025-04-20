// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AssistantNest.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AssistantNest.Extensions;

public static class HttpContextExtensions
{
    public static async Task<Guid?> GetUserIdFromCookieAsync(this HttpContext httpContext,
        ILogger logger, CancellationToken cancellationToken = default)
    {
        string? idString = await Task.Run(() => 
            httpContext.Request.Cookies.FirstOrDefault(c => c.Key.Equals(Constants.UserSessionCookieKey)).Value, cancellationToken);
        if(idString is null)
        {
            logger.LogWarning("User id not found in cookies");
            return null;
        }
        Guid id = new Guid(idString);
        logger.LogDebug("Retrieved id {Id} from cookie", id);
        return id;
    }

    public static void SetUserIdCookie(this HttpContext httpContext, Guid id)
    {
        httpContext.Response.Cookies.Append(Constants.UserSessionCookieKey, id.ToString(), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
    }

    public static void DeleteUserIdFromCookies(this HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(Constants.UserSessionCookieKey);
    }

    public static async Task<bool> ReauthenticateAsync(this HttpContext httpContext, ClaimsPrincipal user,
        bool acceptedCookies, ILogger logger)
    {
        await httpContext.SignOutAsync();
        logger.LogInformation("Signed out previous user");
        if (!acceptedCookies)
        {

            httpContext.DeleteUserIdFromCookies();
            logger.LogInformation("User rejected cookies");
            return false;
        }
        await httpContext.AuthenticateAsync();
        await httpContext.SignInAsync(user);
        Guid userId = user.GetId();
        logger.LogInformation("User signed in");
        httpContext.SetUserIdCookie(userId);
        logger.LogInformation("set cookie with value {Id}", userId);
        return true;
    }
}
