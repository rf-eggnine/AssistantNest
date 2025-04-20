// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AssistantNest.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AssistantNest.Repositories;

internal class UserRepository : IRepository<AnUser>
{
    private readonly AnDbContext _anDbContext;
    private readonly ILogger _logger;

    public UserRepository(AnDbContext anDbContext, ILogger<UserRepository> logger)
    {
        _anDbContext = anDbContext;
        _logger = logger;
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

    public async Task<AnUser?> GetAsync(Expression<Func<AnUser, bool>> query, CancellationToken cancellationToken = default)
    {
        return await _anDbContext.Users.SingleOrDefaultAsync(query, cancellationToken);
    }

    public async Task<IEnumerable<AnUser>> GetManyAsync(Expression<Func<AnUser, bool>> query, CancellationToken cancellationToken = default)
    {
        return await _anDbContext.Users.Where(query).ToListAsync(cancellationToken);
    }
    public async Task<AnUser?> UpdateAsync(Expression<Func<AnUser, bool>> query, Action<AnUser> update, CancellationToken cancellationToken = default)
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
    public async Task<AnUser?> DeleteAsync(Expression<Func<AnUser, bool>> query, CancellationToken cancellationToken = default)
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


    internal static void UpdateUserCookieAccptance(AnUser anUser, bool acceptedCookies)
    {
        DateTime now = DateTime.UtcNow;
        anUser.AcceptedCookiesAt = acceptedCookies ? now : null;
        anUser.UpdatedAt = now;
    }
}
