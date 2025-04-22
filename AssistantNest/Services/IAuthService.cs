
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AssistantNest.Models;
using Microsoft.AspNetCore.Http;

namespace AssistantNest.Services;

public interface IAuthService
{
    public Task<AnUser?> SignInUserAsync(HttpContext httpContext, bool acceptedCookies = false, 
        CancellationToken cancellationToken = default);

    public Task<AnUser?> RegisterUserAsync(HttpContext httpContext, string name, string password, 
        CancellationToken cancellationToken = default);

    public Task<AnUser?> AuthenticateWithCredentialsAsync(HttpContext httpContext, string name, string password,
        CancellationToken cancellationToken = default);    
    
}