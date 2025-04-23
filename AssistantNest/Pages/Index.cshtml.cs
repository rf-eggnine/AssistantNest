// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using AssistantNest.Repositories;
using AssistantNest.Models;
using AssistantNest.Extensions;
using System;
using AssistantNest.Services;

namespace AssistantNest.Pages;
public class Index : PageModel
{
    private readonly ILogger _logger;
    private readonly IRepository<AnUser> _users;
    private readonly IAuthService _authService;
    public Index(ILogger<Index> logger, IRepository<AnUser> users, IAuthService authService)
    {
        _authService = authService;
        _logger = logger;
        _users = users;
    }

    public AnUser? AnUser {get;set;}

    public async Task<IActionResult> OnGetAsync(bool acceptedCookies = false, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Path {Path}", HttpContext.Request.Path);
        AnUser = await HttpContext.GetUserFromCookieAsync(_users, _logger, cancellationToken);
        if(AnUser is not null && AnUser.HasAcceptedCookies && acceptedCookies)
        {
            return RedirectPreserveMethod($"Home?{Constants.QueryStringKeyAcceptedCookies}=true");
        }
        return Page();
    }
}
