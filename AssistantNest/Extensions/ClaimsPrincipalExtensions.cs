// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AssistantNest.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static async Task<Guid?> GetIdAsync(this ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken = default)
    {
        string? id = (await claimsPrincipal.Claims.AsQueryable()
            .SingleOrDefaultAsync(c => c.Type.Equals(Constants.IdClaim), cancellationToken))?.Value;
        if (string.IsNullOrEmpty(id))
        {
            return Guid.Empty;
        }
        if (Guid.TryParse(id, out Guid parsedId))
        {
            return parsedId;
        }
        return null;
    }
}
