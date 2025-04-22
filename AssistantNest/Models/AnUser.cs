// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AssistantNest.Exceptions;
using AssistantNest.Extensions;
using Eggnine.Common;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AssistantNest.Models;

public class AnUser : ClaimsPrincipal
{
    public AnUser(Guid id) : base (new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(Constants.IdClaim, id.ToString())], CookieAuthenticationDefaults.AuthenticationScheme)))
    { }

    public Guid Id => this.GetIdAsync().GetAwaiter().GetResult() ?? throw new AnUserMissingIdClaimException();
    public DateTime EncounteredAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedCookiesAt {get;set;} = null;
    public bool HasAcceptedCookies => AcceptedCookiesAt.HasValue;
    public DateTime? SignedUpAt { get; set; } = DateTime.UtcNow;
    public string? Name { get; set; } = null;
    public string? EncryptedPassphrase { get; set; } = null;
    public ICollection<AnProject> Projects { get; set; } = new List<AnProject>();

    public async Task<bool> VerifyEncryptionAsync(string passphrase, CancellationToken cancellationToken = default) 
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passphrase);
        if(EncryptedPassphrase == null)
        {
            return false;
        }
        return await passphrase.VerifyEncryptionAsync(EncryptedPassphrase, cancellationToken);
    }
}
