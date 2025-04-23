// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using System;
using System.Threading;
using System.Threading.Tasks;
using AssistantNest.Extensions;
using AssistantNest.Models;
using AssistantNest.Repositories;
using Eggnine.Common;
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

    public async Task<AnUser?> SignInUserAsync(HttpContext httpContext, bool acceptedCookies = false, CancellationToken cancellationToken = default)
    {
        AnUser? user = await httpContext.GetUserFromCookieAsync(_userRepository, _logger, cancellationToken);
        if (user is not null)
        {
            return await HandleExistingUserAsync(httpContext, user, acceptedCookies, cancellationToken);
        }

        Guid? cookieUserId = httpContext.GetUserIdFromCookie(_logger);
        if (cookieUserId is not null)
        {
            return await TryCreateUserFromCookieAsync(httpContext, cookieUserId.Value, acceptedCookies, cancellationToken);
        }

        // No cookie at all
        _logger.LogInformation("UserId not found in cookies");
        if (!acceptedCookies)
        {
            _logger.LogInformation("User rejected cookies. Skipping user creation.");
            return null;
        }

        Guid newUserId = Guid.NewGuid();
        _logger.LogInformation("Generated new userId {Id}", newUserId);
        AnUser? newUser = await CreateNewUserAsync(newUserId, acceptedCookies, cancellationToken);
        if (newUser is null)
        {
            return null;
        }

        await httpContext.SignInAsync(newUser);
        _logger.LogInformation("User signed in with id {Id}", newUserId);
        return newUser;
    }

    public async Task<AnUser?> RegisterUserAsync(HttpContext httpContext, string name, string password, CancellationToken cancellationToken = default)
    {
        name = name.Trim();
        password = password.Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Username or password is empty");
            return null;
        }
        if (name.Length < 3 || name.Length > 20)
        {
            _logger.LogWarning("Username '{Name}' is not between 3 and 20 characters", name);
            return null;
        }
        if (password.Length < 8 || password.Length > 100)
        {
            _logger.LogWarning("Password is not between 8 and 100 characters");
            return null;
        }
        // Check if name already exists
        var existingUser = await _userRepository.GetAsync(u => name.Equals(u.Name), cancellationToken);
        if (existingUser != null)
        {
            _logger.LogWarning("Username '{Name}' is already taken", name);
            return null;
        }

        // Create update user with encrypted password
        AnUser? userToUpdate = await httpContext.GetUserFromCookieAsync(_userRepository, _logger, cancellationToken);
        if (userToUpdate is null)
        {
            _logger.LogWarning("UserId not found in cookies");
            return null;
        }
        if (userToUpdate.Name != null)
        {
            _logger.LogWarning("User already has a name");
            return null;
        }
        DateTime now = DateTime.UtcNow;

        string encryptedPassword = await password.EncryptAsync(cancellationToken);
        AnUser? updatedUser = await _userRepository.UpdateAsync(u => u.Id == userToUpdate.Id, u =>
        {
            u.Name = name;
            u.EncryptedPassphrase = encryptedPassword;
            u.UpdatedAt = now;
        }, cancellationToken);
        
        if (updatedUser == null)
        {
            _logger.LogWarning("Failed to register user '{Name}'", name);
            return null;
        }

        _logger.LogInformation("User registered with id {Id}", updatedUser.Id);
        await httpContext.SignInAsync(updatedUser);
        _logger.LogInformation("User signed in with id {Id}", updatedUser.Id);
        return updatedUser;
    }

    public async Task<AnUser?> AuthenticateWithCredentialsAsync(HttpContext httpContext, string name, string password, CancellationToken cancellationToken = default)
    {
        name = name.Trim();
        password = password.Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Username or password is empty");
            return null;
        }
        
        AnUser? user = await _userRepository.GetAsync(u => name.Equals(u.Name), cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Login failed: user '{Name}' not found", name);
            return null;
        }

        if (!await password.VerifyEncryptionAsync(user.EncryptedPassphrase ?? string.Empty, cancellationToken))
        {
            _logger.LogWarning("Login failed: invalid password for user '{Name}'", name);
            return null;
        }

        _logger.LogInformation("User '{Name}' authenticated", name);
        await httpContext.SignInAsync(user);
        return user;
    }


    private async Task<AnUser?> HandleExistingUserAsync(HttpContext httpContext, AnUser user, bool acceptedCookies, CancellationToken cancellationToken)
    {
        Guid userId = user.Id;
        _logger.LogInformation("User already signed in with id {Id}", userId);

        var result = await httpContext.AuthenticateAsync(Constants.AuthScheme);
        if (!result.Succeeded || !result.Principal.GetId().Equals(userId))
        {
            _logger.LogInformation("User with id {Id} failed reauthentication, signing out", userId);
            await httpContext.SignOutAsync(Constants.AuthScheme);
            return null;
        }
        if (!acceptedCookies)
        {
            _logger.LogInformation("Signed in user has now rejected cookies");
            await _userRepository.UpdateAsync(u => u.Id == userId, u => UpdateUserCookieAcceptance(u, false), cancellationToken);
            await httpContext.SignOutAsync(Constants.AuthScheme);
            _logger.LogInformation("User with id {Id} signed out", userId);
            return null;
        }

        if (!user.HasAcceptedCookies)
        {
            _logger.LogInformation("User with id {Id} now accepts cookies", userId);
            await _userRepository.UpdateAsync(u => u.Id == userId, u => UpdateUserCookieAcceptance(u, true), cancellationToken);
        }
        await httpContext.SignInAsync(user);
        _logger.LogInformation("User with id {Id} signed in", userId);
        return user;
    }

    private async Task<AnUser?> TryCreateUserFromCookieAsync(HttpContext httpContext, Guid userId, bool acceptedCookies, CancellationToken cancellationToken)
    {
        _logger.LogInformation("UserId {Id} found in cookie", userId);
        AnUser? user = await CreateNewUserAsync(userId, acceptedCookies, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("User not added with id {Id}", userId);
            await httpContext.SignOutAsync(Constants.AuthScheme);
            return null;
        }

        var result = await httpContext.AuthenticateAsync(Constants.AuthScheme);
        if (result.Succeeded)
        {
            _logger.LogInformation("User with id {Id} authenticated", userId);
            return user;
        }

        _logger.LogInformation("User with id {Id} failed auth, signing out", userId);
        await httpContext.SignOutAsync(Constants.AuthScheme);
        return null;
    }

    private async Task<AnUser?> CreateNewUserAsync(Guid newUserId, bool acceptedCookies, CancellationToken cancellationToken)
    {
        if (!acceptedCookies)
        {
            _logger.LogInformation("User rejected cookies. Not creating user.");
            return null;
        }

        _logger.LogInformation("Creating new user with id {Id}", newUserId);
        DateTime now = DateTime.UtcNow;

        AnUser? user = await _userRepository.AddAsync(new AnUser(newUserId)
        {
            EncounteredAt = now,
            UpdatedAt = now,
            AcceptedCookiesAt = now
        }, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Failed to add user with id {Id}", newUserId);
            return null;
        }

        _logger.LogInformation("User added with id {Id}", user.Id);
        return user;
    }

    internal static void UpdateUserCookieAcceptance(AnUser anUser, bool acceptedCookies)
    {
        DateTime now = DateTime.UtcNow;
        anUser.AcceptedCookiesAt = acceptedCookies ? now : default;
        anUser.UpdatedAt = now;
    }
}