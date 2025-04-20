// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.

using AssistantNest.Models;
using AssistantNest.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AssistantNest.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnDbContext(this IServiceCollection services)
    {
        services.AddDbContextFactory<AnDbContext>((optionsBuilder) =>
        {
            ((DbContextOptionsBuilder<AnDbContext>)optionsBuilder)
                .UseNpgsql("Host=localhost;Database=AssistantNest;Username=postgres;Password=postgres");
        });
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<AnDbContext>();
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IRepository<AnUser>, UserRepository>();
        return services;
    }
}
