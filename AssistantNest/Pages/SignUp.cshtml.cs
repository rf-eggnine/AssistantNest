// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using Eggnine.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;
using AssistantNest.Validations;
using AssistantNest.Repositories;
using AssistantNest.Models;
using AssistantNest.Extensions;

namespace AssistantNest.Pages;
public class SignUp : PageModel
{
    private readonly ILogger _logger;
    private readonly IUserRepository _users;

    public SignUp(ILogger<SignUp> logger, IUserRepository users)
    {
        _logger = logger;
        _users = users;
    }

    public IList<IValidation> Validations {get;set;} = new List<IValidation>();
    
    public async Task<AnUser?> GetUserFromCookieAsync(CancellationToken cancellationToken = default)
    {
        return await HttpContext.GetUserFromCookieAsync(_users, _logger, cancellationToken);
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await _users.SignInUserAsync(HttpContext, false, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string name, string passphrase, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(passphrase);
        Validations.Clear();
        string encrypted = await passphrase.EncryptAsync(cancellationToken);
        AnUser? anUser = await HttpContext.GetUserFromCookieAsync(_users, _logger, cancellationToken);
        if(anUser is null)
        {
            _logger.LogInformation("No User currently connected, logging in new User");
            return Page();
        }
        if(!await _users.UpdateAsync(p => p.Id.Equals(anUser.Id), p =>
            {
                p.Name = name;
                p.EncryptedPassphrase = encrypted;
            }))
        {
            _logger.LogInformation("Update User failed");
            Validations.Add(new UserAlreadyExistsValidation());
            return Page();
        }
        return RedirectToPage("home");
    }
}
