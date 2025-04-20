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
            AnUser? anUser = await httpContext.GetUserFromCookieAsync(this, _logger, cancellationToken);
            if(anUser is null)
            {
                anUser = new AnUser(Guid.NewGuid(), acceptedCookies);
                anUser.HasAcceptedCookies = acceptedCookies;
                if(await AddAsync(anUser, cancellationToken))
                {
                    _logger.LogInformation("User added with id {Id}", anUser.Id);
                    await httpContext.AuthenticateAsync();
                    await Task.Run(async () => await httpContext.SignInAsync(anUser), cancellationToken);
                    _logger.LogInformation("User signed in");
                    httpContext.Response.Cookies.Append(Constants.UserSessionCookieKey, anUser.Id.ToString());
                    _logger.LogInformation("set cookie with value {id}", anUser.Id);
                    return anUser;
                }
                _logger.LogWarning("User not signed in with id {Id}", anUser.Id);
                return null;
            }
            _logger.LogInformation("Initial User was not null");
            return anUser;
        }
        finally
        {
            _logger.LogTrace("Exiting {MethodName}", nameof(SignInUserAsync));
        }
    }
    public async Task<AnUser?> SignInUserAsync(HttpContext httpContext, Guid id, 
        CancellationToken cancellationToken = default)
    {
        await httpContext.SignOutAsync();
        AnUser? anUser = await GetAsync(u => u.Id.Equals(id), cancellationToken);
        if(anUser is not null)
        {
            _logger.LogInformation("User found with id {Id}", anUser.Id);
            await httpContext.AuthenticateAsync();
            await Task.Run(async () => await httpContext.SignInAsync(anUser), cancellationToken);
            _logger.LogInformation("User signed in");
            httpContext.Response.Cookies.Append(Constants.UserSessionCookieKey, anUser.Id.ToString());
            _logger.LogInformation("set cookie with value {id}", anUser.Id);
            return anUser;
        }
        return null;
    }

    public Task<bool> AddAsync(AnUser anUser, CancellationToken cancellationToken = default)
    {
        if (_anDbContext.Users.Any(u => u.Id.Equals(anUser.Id)))
        {
            _logger.LogWarning("User not stored because of conflict with id {Id}", anUser.Id);
            return Task.FromResult(false);
        }
        _anDbContext.Users.Add(anUser);
        _anDbContext.SaveChanges();
        return Task.FromResult(true);
    }

    public async Task<AnUser?> GetAsync(Func<AnUser, bool> query, CancellationToken cancellationToken = default)
    {

        return await Task.Run(() => _anDbContext.Users.SingleOrDefault(query), cancellationToken);
    }

    public async Task<IEnumerable<AnUser>> GetManyAsync(Func<AnUser, bool> query, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => _anDbContext.Users.Where(query), cancellationToken);
    }
    public Task<bool> UpdateAsync(Func<AnUser, bool> query, Action<AnUser> update, CancellationToken cancellationToken = default)
    {
        _anDbContext.Users.Where(query).ToList().ForEach(update);
        List<string?> names = _anDbContext.Users.Select(u => u.Name).ToList();
        if(names.Count != names.Distinct().Count())
        {
            _logger.LogWarning("Could not update User because of conflicting User");
            return Task.FromResult(false);
        }
        _anDbContext.SaveChanges();
        return Task.FromResult(true);
    }
}
