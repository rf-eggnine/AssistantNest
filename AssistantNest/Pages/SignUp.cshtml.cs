// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
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
using AssistantNest.Services;

namespace AssistantNest.Pages;
public class SignUp : PageModel
{
    private readonly ILogger _logger;
    private readonly IRepository<AnUser> _users;
    private readonly IAuthService _authService;
    public SignUp(ILogger<SignUp> logger, IRepository<AnUser> users, IAuthService authService)
    {
        _authService = authService;
        _logger = logger;
        _users = users;
    }

    public AnUser? AnUser {get;set;}
    public IList<IValidation> Validations {get;set;} = new List<IValidation>();

    [BindProperty]
    public string Name { get; set; } = string.Empty;
    [BindProperty]
    public string Passphrase { get; set; } = string.Empty;
    
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        AnUser = await _authService.SignInUserAsync(HttpContext, false, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(Passphrase);
        Validations.Clear();
        AnUser = await _authService.RegisterUserAsync(
            HttpContext,
            Name,
            Passphrase,
            cancellationToken
        );
        if (AnUser is null)
        {
            _logger.LogInformation("User not found");
            Validations.Add(new UserAlreadyExistsValidation());
            return Page();
        }
        return RedirectToPage("home");
    }
}
