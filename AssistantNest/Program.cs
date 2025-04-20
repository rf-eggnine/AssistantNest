// ©️ 2025 RF@Eggnine.com
// Licensed under the EG9-PD License which includes a personal IP disclaimer.
// See LICENSE file in the project root for full license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NReco.Logging.File;
using AssistantNest.Extensions;
using AssistantNest.OpenApi;

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
        builder.Configuration
            .AddJsonFile("appSettings.json",
                optional: true,
                reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddCommandLine(args);
        builder.Logging
            .AddConsole()
            .AddDebug();
        builder.Services
            .AddAntiforgery()
            .AddAuthentication().AddCookie();
        builder.Services
            .AddAuthorization()
            .AddDbContextFactory<AnDbContext>((optionsBuilder) =>
            {
                ((DbContextOptionsBuilder<AnDbContext>)optionsBuilder)
                    .UseNpgsql("Server=localhost;Port=5432;User Id=an;Password=REMOVED;Database=an;")
                    .UseModel(AnDbContext.GetModelBuilder().FinalizeModel());
            })
            .AddScoped<AnDbContext>()
            .AddRepositories()
            .AddRazorPages();
        builder.Services
            .AddLogging(loggingBuilder => loggingBuilder.AddFile("an.log", append: true));
        builder.Services
            .AddRouting();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Assistant Nest API", Version = "v1" });
                c.SchemaFilter<AnProjectsSchemaFilter>();
                c.AddOperationFilterInstance(new GetProjectsFilter());
                c.AddDocumentFilterInstance(new RemoveVoidSchemaDocumentFilter());
            });
        }
        else
        {
            builder.Services.AddHsts(options =>
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
        app.UseMyCookiePolicy();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAntiforgery();
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapRazorPages()
            .RequireAuthorization()
            .WithMetadata();

        return app;
    }
}
