// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using AssistantNest.Repositories;
using AssistantNest.Models;
using AssistantNest.Extensions;
using AssistantNest.Validations;

namespace AssistantNest.Pages;
public class SignIn : PageModel
{
    private readonly ILogger _logger;
    private readonly IUserRepository _users;

    public SignIn(ILogger<SignIn> logger, IUserRepository users)
    {
        _logger = logger;
        _users = users;
    }
    
    public async Task<AnUser?> GetUserFromCookieAsync(CancellationToken cancellationToken = default)
    {
        return await HttpContext.GetUserFromCookieAsync(_users, _logger, cancellationToken);
    }

    public IList<IValidation> Validations {get;set;} = new List<IValidation>();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await _users.SignInUserAsync(HttpContext, false, cancellationToken);
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(string name, string passphrase, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(passphrase);
        Validations.Clear();
        AnUser? anUser = await _users.GetAsync(u => name.Equals(u.Name), cancellationToken);
        if(anUser is null)
        {
            Validations.Add(new UserNotFoundValidation());
            return Page();
        }
        if(!await anUser.VerifyEncryptionAsync(passphrase, cancellationToken))
        {
            Validations.Add(new UserNotFoundValidation());
            return Page();
        }
        await _users.SignInUserAsync(HttpContext, anUser.Id, cancellationToken);
        return RedirectToPage("home");
    }
}
