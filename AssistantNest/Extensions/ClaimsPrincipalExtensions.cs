// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using System.Security.Claims;

namespace AssistantNest.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetId(this ClaimsPrincipal claimsPrincipal)
    {
        string? id = claimsPrincipal.Claims.FirstOrDefault(c => c.Type.Equals(Constants.IdClaim))?.Value;
        return id == null ? Guid.Empty : new Guid(id);
    }
}
