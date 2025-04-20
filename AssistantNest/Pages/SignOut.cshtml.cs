// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using AssistantNest.Validations;
using AssistantNest.Repositories;
using AssistantNest.Models;
using AssistantNest.Extensions;

namespace AssistantNest.Pages;
public class SignOut : PageModel
{
    private readonly ILogger _logger;
    private readonly IUserRepository _users;

    public SignOut(ILogger<SignOut> logger, IUserRepository users)
    {
        _logger = logger;
        _users =  users;
    }

    public IList<IValidation> Validations {get;set;} = new List<IValidation>();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Entering {MethodName}", nameof(OnGetAsync));
        await HttpContext.SignOutAsync();
        HttpContext.Response.Cookies.Delete(Constants.UserSessionCookieKey);
        return RedirectToPage("/Index");
    }
}
