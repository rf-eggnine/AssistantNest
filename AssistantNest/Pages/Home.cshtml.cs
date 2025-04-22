// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssistantNest.Models;
using AssistantNest.Repositories;
using AssistantNest.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AssistantNest.Pages;
public class Home : PageModel
{
    private readonly ILogger _logger;
    private readonly IRepository<AnUser> _users;
    private readonly IAuthService _authService;
    public Home(ILogger<Home> logger, IRepository<AnUser> users, IAuthService authService)
    {
        _authService = authService;
        _logger = logger;
        _users = users;
    }

    public AnUser? AnUser {get;set;}

    public async Task<IActionResult> OnGetAsync(bool acceptedCookies = false, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Entering {MethodName}", nameof(OnGetAsync));
        AnUser = await _authService.SignInUserAsync(HttpContext, acceptedCookies, cancellationToken);
        if(acceptedCookies)
        {
            HttpContext.Response.Headers.Append(Constants.HeaderAcceptedCookies, "true");
            return RedirectToPage("Home");
        };
        return Page();
    }
}
