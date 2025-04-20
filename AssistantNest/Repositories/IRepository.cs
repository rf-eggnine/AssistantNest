// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AssistantNest.Repositories;

public interface IRepository<T>
{
    public Task<T?> GetAsync(Expression<Func<T, bool>> query, CancellationToken cancellationToken = default);
    public Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> query, CancellationToken cancellationToken = default);

    public Task<T?> AddAsync(T t, CancellationToken cancellationToken = default);

    public Task<T?> UpdateAsync(Expression<Func<T, bool>> query, Action<T> update, CancellationToken cancellationToken = default);

    public Task<T?> DeleteAsync(Expression<Func<T, bool>> query, CancellationToken cancellationToken = default);
}
