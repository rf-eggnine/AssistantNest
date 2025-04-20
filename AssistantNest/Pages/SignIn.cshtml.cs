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
using AssistantNest.Services;

namespace AssistantNest.Pages;
public class SignIn : PageModel
{
    private readonly ILogger _logger;
    private readonly IRepository<AnUser> _users;
    private readonly IAuthService _authService;
    public SignIn(ILogger<SignIn> logger, IRepository<AnUser> users, IAuthService authService)
    {
        _authService = authService;
        _logger = logger;
        _users = users;
    }

    public AnUser? AnUser { get; set; }
    public IList<IValidation> Validations {get;set;} = new List<IValidation>();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        AnUser = await _authService.SignInUserAsync(HttpContext, false, cancellationToken);
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
        HttpContext.SetUserIdCookie(anUser.Id);
        AnUser = await _authService.SignInUserAsync(HttpContext, true, cancellationToken);
        return RedirectToPage("home");
    }
}
