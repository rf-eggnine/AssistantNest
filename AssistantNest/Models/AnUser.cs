// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AssistantNest.Extensions;
using Eggnine.Common;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AssistantNest.Models;

public class AnUser : ClaimsPrincipal
{
    public AnUser(Guid id, bool hasAcceptedCookies) : this(id, null, hasAcceptedCookies, null)
    {
    }
    
    public AnUser(Guid id, string? name, bool hasAcceptedCookies, string? encryptedPassphrase) : 
        base (new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(Constants.IdClaim, id.ToString())], CookieAuthenticationDefaults.AuthenticationScheme)))
    {
        HasAcceptedCookies = hasAcceptedCookies;
        NameArray = Array.Empty<char>();
        Name = name;
        EncryptedPassphraseArray = Array.Empty<char>();
        EncryptedPassphrase = encryptedPassphrase;
    }
    public Guid Id => this.GetId();
    internal char[] NameArray {get;set;}
    public bool HasAcceptedCookies {get;set;}
    internal char[] EncryptedPassphraseArray {get;set;}
    public string? Name
    {
        get => NameArray.Length == 0 ? null : string.Join(string.Empty, NameArray);
        set
        {
            if(value is null)
            {
                NameArray = Array.Empty<char>();
            }
            else
            {
                NameArray = value.ToCharArray();
            }
        }
    }

    public string? EncryptedPassphrase
    {
        get => EncryptedPassphraseArray.Length == 0 ? null : string.Join(string.Empty, EncryptedPassphraseArray);
        set
        {
            if(value is null)
            {
                EncryptedPassphraseArray = Array.Empty<char>();
            }
            else
            {
                EncryptedPassphraseArray = value.ToCharArray();
            }
        }
    }


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
