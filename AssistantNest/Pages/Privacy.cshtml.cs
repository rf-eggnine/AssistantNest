// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using AssistantNest.Repositories;
using AssistantNest.Models;
using AssistantNest.Extensions;
using Eggnine.SentientHorizon.Web.Pages;

namespace AssistantNest.Pages;
public class Privacy : PageModel
{
    private readonly ILogger _logger;
    private readonly IUserRepository _users;

    public Privacy(ILogger<Privacy> logger, IUserRepository users)
    {
        _logger = logger;
        _users = users;
    }

    public AnUser? AnUser {get;set;}

    public async Task<AnUser?> GetUserFromCookieAsync(CancellationToken cancellationToken = default)
    {
        return await HttpContext.GetUserFromCookieAsync(_users, _logger, cancellationToken);
    }

    public async Task<IActionResult> OnGetAsync(bool acceptedCookies = false, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Entering {ClassName}.{MethodName}", nameof(Privacy), nameof(OnGetAsync));
        AnUser = await GetUserFromCookieAsync(cancellationToken);
        return Page();
    }
}
