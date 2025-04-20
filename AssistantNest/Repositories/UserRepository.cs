// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AssistantNest.Models;
using AssistantNest.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AssistantNest.Repositories;

internal class UserRepository : IUserRepository
{
    private readonly AnDbContext _anDbContext;
    private readonly ILogger _logger;

    public UserRepository(AnDbContext anDbContext, ILogger<UserRepository> logger)
    {
        _anDbContext = anDbContext;
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
            AnUser? anUser = await GetAsync(u => u.Id.Equals(userId), cancellationToken);
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
        AnUser? anUser = await AddAsync(new AnUser(newUserId)
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
        if (await UpdateAsync(u => u.Id.Equals(userId), u => 
            {
                u.AcceptedCookiesAt = null;
                u.UpdatedAt = DateTime.UtcNow;
            }, cancellationToken) is null)
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
        if (await UpdateAsync(u => u.Id.Equals(userId), u => 
            {
                u.AcceptedCookiesAt = acceptedCookiesAtAndUpdatedAt;
                u.UpdatedAt = acceptedCookiesAtAndUpdatedAt;
            }, cancellationToken) is null)
        {
            Exception exception = new Exception("Could not update user");
            _logger.LogError(exception, "Could not update user with id {Id}", userId);
            return null;
        }
        AnUser? toReturn = await GetAsync(u => u.Id.Equals(userId), cancellationToken);
        if (toReturn is null)
        {
            _logger.LogError("User not found with id {Id}", userId);
            return null;
        }
        else if (httpContext.User is AnUser currentAnUser)
        {
            if (currentAnUser.Id.Equals(toReturn.Id))
            {
                return toReturn;
            }
            await httpContext.SignOutAsync();
            _logger.LogInformation("User signed out");
        }
        else if (httpContext.User is not null)
        {
            await httpContext.SignOutAsync();
        }
        await httpContext.AuthenticateAsync();
        _logger.LogInformation("User authenticated");
        await httpContext.SignInAsync(toReturn);
        _logger.LogInformation("User signed in");
        httpContext.SetUserIdCookie(toReturn.Id);
        _logger.LogInformation("set cookie with value {Id}", toReturn.Id);
        return toReturn;
    }

    public async Task<AnUser?> AddAsync(AnUser anUser, CancellationToken cancellationToken = default)
    {
        if (await GetAsync(u => u.Id.Equals(anUser.Id), cancellationToken) is not null)
        {
            _logger.LogWarning("User not stored because of conflict with id {Id}", anUser.Id);
            return null;
        }
        await _anDbContext.Users.AddAsync(anUser, cancellationToken);
        await _anDbContext.SaveChangesAsync(cancellationToken);
        return anUser;
    }

    public async Task<AnUser?> GetAsync(Func<AnUser, bool> query, CancellationToken cancellationToken = default)
    {
        return await _anDbContext.Users.SingleOrDefaultAsync(u => query(u), cancellationToken);
    }

    public async Task<IEnumerable<AnUser>> GetManyAsync(Func<AnUser, bool> query, CancellationToken cancellationToken = default)
    {
        return await _anDbContext.Users.Where(u => query(u)).ToListAsync(cancellationToken);
    }
    public async Task<AnUser?> UpdateAsync(Func<AnUser, bool> query, Action<AnUser> update, CancellationToken cancellationToken = default)
    {
        AnUser? anUser = await GetAsync(query, cancellationToken);
        if (anUser is null)
        {
            return null;
        }
        update(anUser);
        if (await GetAsync(u => u.Id != anUser.Id && string.Equals(u.Name, anUser.Name, StringComparison.CurrentCultureIgnoreCase), cancellationToken) is not null)
        {
            _logger.LogWarning("Could not update User because of conflicting User");
            return null;
        }
        await _anDbContext.SaveChangesAsync(cancellationToken);
        return anUser;
    }
    public async Task<AnUser?> DeleteAsync(Func<AnUser, bool> query, CancellationToken cancellationToken = default)
    {
        AnUser? anUser = await GetAsync(query, cancellationToken);
        if (anUser is null)
        {
            return null;
        }
        _anDbContext.Users.Remove(anUser);
        await _anDbContext.SaveChangesAsync(cancellationToken);
        return anUser;
    }
}
