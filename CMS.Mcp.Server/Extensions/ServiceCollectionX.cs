namespace CMS.Mcp.Server.Extensions
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Options;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using ModelContextProtocol.AspNetCore.Authentication;
    using Monkey;
    using Shared.Common;

    public static class ServiceCollectionX
    {
        public static IServiceCollection AddMcpServer(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddHttpClient()
                .AddSecurity(configuration)
                .AddMcp()
                .AddServices();
        }

        public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            var identityServerSection = configuration.GetSection(ConfigSectionNames.IDENTITY_SERVER);
            var identityServer = identityServerSection.Get<IdentityServerOptions>();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o =>
                {
                    o.Authority = identityServer.Authority.ToString();
                    o.RequireHttpsMetadata = identityServer.JwtBearer.RequireHttpsMetadata;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidAudiences = identityServer.JwtBearer.ValidAudiences,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        LifetimeValidator = (_, expires, _, _) => expires != null && expires > DateTime.UtcNow
                    };
                    o.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();

                            logger.LogInformation($"Token validated, Token : {context.SecurityToken.ToJson()}");
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();

                            logger.LogError(context.Exception, "Authentication failed");
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();

                            logger.LogError($"Challenging client to authenticate, Error : {context.Error ?? string.Empty}, " +
                                            $"Description : {context.ErrorDescription ?? context.ErrorDescription}, " +
                                            $"ErrorUri : {context.ErrorUri ?? string.Empty}");
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddMcp(options =>
                {
                    options.ResourceMetadata = new()
                    {
                        Resource = new Uri("https://localhost:5243"),
                        AuthorizationServers = { new Uri(identityServer.Authority.ToString()) },
                        ScopesSupported = ["mcp:tools"],
                    };
                });

            services.AddAuthorization();

            return services;
        }

        public static IServiceCollection AddMcp(this IServiceCollection services)
        {
            services
                .AddMcpServer()
                .WithHttpTransport()
                //.WithPrompts<MonkeyPrompts>()
                //.WithResources<MonkeyResources>()
                .WithTools<MonkeyTools>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<MonkeyService>()
                .AddSingleton<MonkeyLocationService>();
        }
    }
}