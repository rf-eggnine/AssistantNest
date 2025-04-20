// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssistantNest.Models;
using AssistantNest.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AssistantNest.Extensions;

public static class HttpContextExtensions
{
    public static async Task<AnUser?> GetUserFromCookieAsync(this HttpContext httpContext, IUserRepository users,
        ILogger logger, CancellationToken cancellationToken = default)
    {
        string? idString = await Task.Run(() => 
            httpContext.Request.Cookies.FirstOrDefault(c => c.Key.Equals(Constants.UserSessionCookieKey)).Value);
        if(idString is null)
        {
            logger.LogWarning("User id not found in cookies");
            return null;
        }
        Guid id = new Guid(idString);
        logger.LogDebug("Retrieved id {Id} from cookie", id);
        return await users.GetAsync(u => u.Id.Equals(id), cancellationToken);
    }
}
