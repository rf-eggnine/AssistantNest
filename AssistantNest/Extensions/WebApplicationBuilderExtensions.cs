
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AssistantNest.Extensions;

internal static class WebApplicationBuilderExtensions
{
    internal static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder, Action<IConfigurationBuilder> configure)
    {
        configure(builder.Configuration);
        return builder;
    }
    
    internal static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder, Action<ILoggingBuilder> configure)
    {
        builder.Services.AddLogging(loggingBuilder => configure(loggingBuilder));
        return builder;
    }

    internal static WebApplicationBuilder AddServices(this WebApplicationBuilder builder, Action<IServiceCollection> configureServices)
    {
        configureServices(builder.Services);
        return builder;
    }

    internal static IServiceCollection AddMyCookieAuthenticationScheme(this IServiceCollection services, string authScheme, 
        Action<CookieAuthenticationOptions> configureCookieAuthOptions)
    {
        services.AddAuthentication(authScheme)
            .AddCookie(authScheme, authOptions => configureCookieAuthOptions(authOptions));
        return services;
    }

    internal static IServiceCollection AddMyRazorPages(this IServiceCollection services)
    {
        services.AddRazorPages();
        return services;
    }
}