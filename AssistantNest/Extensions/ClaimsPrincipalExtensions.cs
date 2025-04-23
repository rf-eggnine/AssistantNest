// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using System.Security.Claims;

namespace AssistantNest.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetId(this ClaimsPrincipal claimsPrincipal)
    {
        var idClaim = claimsPrincipal.Claims
            .SingleOrDefault(c => c.Type == Constants.IdClaim);

        if (string.IsNullOrWhiteSpace(idClaim?.Value))
            return null;

        return Guid.TryParse(idClaim.Value, out var parsedId) ? parsedId : null;
    }
}
