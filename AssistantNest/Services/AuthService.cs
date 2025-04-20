
using System;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AssistantNest.Extensions;
using AssistantNest.Models;
using AssistantNest.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AssistantNest.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<AnUser> _userRepository;
    private readonly ILogger _logger;

    public AuthService(IRepository<AnUser> userRepository, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<AnUser?> SignInUserAsync(HttpContext httpContext, bool acceptedCookies = false, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guid? userId = await httpContext.GetUserIdFromCookieAsync(_logger, cancellationToken);
            if (userId is null)
            {
                _logger.LogInformation("UserId not found in cookies");
                userId = Guid.NewGuid();
                httpContext.SetUserIdCookie(userId.Value);
                _logger.LogInformation("set cookie with value {Id}", userId.Value);
                return await CreateNewUserAsync(userId.Value, httpContext, acceptedCookies, cancellationToken);
            }
            AnUser? anUser = await _userRepository.GetAsync(u => u.Id.Equals(userId), cancellationToken);
            if (anUser is null)
            {
                _logger.LogWarning("User not found with id {Id}", userId);
                return await CreateNewUserAsync(userId.Value, httpContext, acceptedCookies, cancellationToken);
            }
            if (anUser.HasAcceptedCookies.Equals(acceptedCookies))
            {
                return anUser;
            }
            if(acceptedCookies)
            {
                return await UpdateUserAcceptsCookiesAsync(anUser, httpContext, cancellationToken);
            }
            return await UpdateUserRejectsCookiesAsync(anUser, httpContext, cancellationToken);
        }
        finally
        {
            _logger.LogTrace("Exiting {MethodName}", nameof(SignInUserAsync));
        }
    }

    private async Task<AnUser?> CreateNewUserAsync(Guid newUserId, HttpContext httpContext, bool acceptedCookies, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new user with id {Id}", newUserId);
        DateTime now = DateTime.UtcNow;
        AnUser? anUser = await _userRepository.AddAsync(new AnUser(newUserId)
            {
                EncounteredAt = now,
                UpdatedAt = now,
                AcceptedCookiesAt = acceptedCookies ? now : null
            }, cancellationToken);
        if (anUser is null)
        {
            _logger.LogWarning("User not added or signed in with id {Id}", newUserId);
            return null;
        }
        _logger.LogInformation("User added with id {Id}", anUser.Id);
        return (await httpContext.ReauthenticateAsync(anUser, acceptedCookies, _logger)) ? anUser : null;
    }

    private async Task<AnUser?> UpdateUserRejectsCookiesAsync(AnUser anUser, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        await httpContext.ReauthenticateAsync(anUser, false, _logger);
        Guid userId = anUser.Id;
        if (await _userRepository.UpdateAsync(u => u.Id.Equals(userId),
                u => UserRepository.UpdateUserCookieAccptance(u, false), cancellationToken) is null)
        {
            Exception exception = new Exception("Could not update user");
            _logger.LogError(exception, "Could not update user with id {Id}", userId);
        }
        return null;
    }

    private async Task<AnUser?> UpdateUserAcceptsCookiesAsync(AnUser anUser, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        DateTime acceptedCookiesAtAndUpdatedAt = DateTime.UtcNow;
        _logger.LogInformation("User accepted cookies");
        Guid userId = anUser.Id;
        if (await _userRepository.UpdateAsync(u => u.Id.Equals(userId),
                u => UserRepository.UpdateUserCookieAccptance(u, true), cancellationToken) is null)
        {
            Exception exception = new Exception("Could not update user");
            _logger.LogError(exception, "Could not update user with id {Id}", userId);
            return null;
        }
        AnUser? toReturn = await _userRepository.GetAsync(u => u.Id.Equals(userId), cancellationToken);
        if (toReturn is null)
        {
            _logger.LogError("User not found with id {Id}", userId);
            return null;
        }
        else if (httpContext.User?.GetId().Equals(toReturn.Id) ?? false)
        {
            return toReturn;
        }
        else if (httpContext.User is not null)
        {
            await httpContext.SignOutAsync();
            _logger.LogInformation("User signed out");
        }
        await httpContext.AuthenticateAsync();
        _logger.LogInformation("User authenticated");
        await httpContext.SignInAsync(toReturn);
        _logger.LogInformation("User signed in");
        httpContext.SetUserIdCookie(toReturn.Id);
        _logger.LogInformation("set cookie with value {Id}", toReturn.Id);
        return toReturn;
    }

}