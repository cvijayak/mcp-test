namespace CMS.Mcp.Server.Extensions
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Options;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
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
                            Console.WriteLine($"Token validated, Token : {context.SecurityToken.ToJson()}");
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                            return Task.CompletedTask;
                        },
                        OnChallenge = _ =>
                        {
                            Console.WriteLine("Challenging client to authenticate with Entra ID");
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddMcp(options =>
                {
                    options.ResourceMetadata = new()
                    {
                        Resource = new Uri("http://localhost:5243"),
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