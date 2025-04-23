// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NReco.Logging.File;
using AssistantNest.Extensions;
using AssistantNest.Models;
using AssistantNest.OpenApi;
using AssistantNest.Pages;
using AssistantNest.Repositories;

namespace AssistantNest;

internal static class Program
{
    private static void Main(string[] args)
    {
        CreateWebApplicationBuilder(args).PreBuildConfigure(args).Build().PostBuildConfigure().Run();
    }

    private static WebApplicationBuilder CreateWebApplicationBuilder(string[] args)
    {
        return WebApplication.CreateEmptyBuilder(new WebApplicationOptions()
        {
            Args = args,
            ApplicationName = "AssistantNest",
            EnvironmentName = GetEnvironmentFromEnvironmentVariables()
        });
    }

    private static string? GetEnvironmentFromEnvironmentVariables()
    {
        string? env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        env ??= Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        return env;
    }

    private static WebApplicationBuilder PreBuildConfigure(this WebApplicationBuilder builder, string[] args)
    {
        builder
            .AddConfiguration(configuration =>
            {
                configuration
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args);
            })
            .AddLogging(loggingBuilder =>
            {
                loggingBuilder
                    .AddFile("an.log", append: true)
                    .AddConsole()
                    .AddDebug();
            })
            .Services
                .AddAntiforgery()
                .AddMyCookieAuthenticationScheme(Constants.AuthScheme, authOptions =>
                {
                    authOptions.Cookie.Name = Constants.UserSessionCookieKey;
                    authOptions.ReturnUrlParameter = "returnUrl";
                    authOptions.LoginPath = $"/{nameof(SignIn)}";
                    authOptions.LogoutPath = $"/{nameof(SignOut)}";
                    //TODO: Add a custom access denied page
                    authOptions.AccessDeniedPath = "/AccessDenied";
                    // authOptions.AccessDeniedPath = $"/{nameof(AccessDenied)}";
                    authOptions.ExpireTimeSpan = TimeSpan.FromDays(30);
                    authOptions.SlidingExpiration = true;
                    authOptions.Cookie.Name = Constants.UserSessionCookieKey;
                    authOptions.Cookie.Path = "/";
                    authOptions.Cookie.SameSite = SameSiteMode.Strict;
                    authOptions.Cookie.HttpOnly = true;
                    authOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    authOptions.Cookie.MaxAge = TimeSpan.FromDays(30);
                })
                .AddAuthorization()
                .AddDbContextFactory<AnDbContext>((optionsBuilder) =>
                {
                    ((DbContextOptionsBuilder<AnDbContext>)optionsBuilder)
                        .UseNpgsql("Server=localhost;Port=5432;User Id=an;Password=;Database=an;")
                        .UseModel(AnDbContext.GetModelBuilder().FinalizeModel());
                })
                .AddScoped<AnDbContext>()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IRepository<AnUser>, UserRepository>()
                .AddMyRazorPages()
                .AddRouting();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services
                .AddEndpointsApiExplorer()
                .AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Assistant Nest API", Version = "v1" });
                    c.SchemaFilter<AnProjectsSchemaFilter>();
                    c.AddOperationFilterInstance(new GetProjectsFilter());
                    c.AddDocumentFilterInstance(new RemoveVoidSchemaDocumentFilter());
                });
        }
        else
        {
            builder.Services
                .AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(60);
                });
        }

        builder.WebHost
            .UseConfiguration(builder.Configuration)
            .ConfigureKestrel(options =>
                options.AddServerHeader = false)
            .UseKestrel();

        return builder;
    }

    private static WebApplication PostBuildConfigure(this WebApplication app)
    {

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app
                .UseSwagger()
                .UseSwaggerUI()
                .UseDeveloperExceptionPage();
        }
        else
        {
            app
                .UseExceptionHandler("/Error")
                .UseHsts();
        }
        app
            .UseMyCookiePolicy()
            .UseStaticFiles()
            .UseRouting()
            .UseAntiforgery()
            .UseAuthentication()
            .UseAuthorization();
        
        app.MapRazorPages()
            .RequireAuthorization()
            .WithMetadata();

        return app;
    }
}
