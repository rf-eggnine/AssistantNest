// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using System;
using System.Threading;
using System.Threading.Tasks;
using AssistantNest.Models;
using Microsoft.AspNetCore.Http;

namespace AssistantNest.Repositories;
public interface IUserRepository : IRepository<AnUser>
{
    public Task<AnUser?> SignInUserAsync(HttpContext httpContext, bool acceptedCookies = false, 
        CancellationToken cancellationToken = default);
    public Task<AnUser?> SignInUserAsync(HttpContext httpContext, 
        Guid id, CancellationToken cancellationToken = default);
}

