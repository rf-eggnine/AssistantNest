// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using System;
using System.Threading;
using System.Threading.Tasks;
using AssistantNest.Models;
using AssistantNest.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AssistantNest.Extensions;

public static class HttpContextExtensions
{
    public static async Task<Guid?> GetUserIdFromCookieAsync(this HttpContext httpContext,
        ILogger logger, CancellationToken cancellationToken = default)
    {
        if (httpContext is null)
        {
            logger.LogError("HttpContext is null");
            return null;
        }
        if (httpContext.User is null)
        {
            logger.LogError("HttpContext.User is null");
            return null;
        }
        return await httpContext.User.GetIdAsync(cancellationToken);
    }

    public static async Task<AnUser?> GetUserFromCookieAsync(this HttpContext httpContext,
        IRepository<AnUser> users, ILogger logger, CancellationToken cancellationToken = default)
    {
        Guid? userId = await httpContext.GetUserIdFromCookieAsync(logger, cancellationToken);
        if (userId is null)
        {
            logger.LogError("UserId is null");
            return null;
        }
        return await users.GetAsync(u => u.Id.Equals(userId), cancellationToken);
    }
}
