using System;
using System.Threading.Tasks;
using CMS.Mcp.Server;
using CMS.Mcp.Server.Contracts;
using CMS.Mcp.Shared.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

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
        o.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidAudiences = identityServer.JwtBearer.ValidAudiences,
            ValidateAudience = true,
            ValidateLifetime = true,
            LifetimeValidator = (_, expires, _, _) => expires != null && expires > DateTime.UtcNow
        };
        o.Events = new JwtBearerEvents {
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
    .AddMcp(options => {
        options.ResourceMetadata = new() {
            Resource = new Uri("http://localhost:5243"),
            AuthorizationServers = { new Uri(identityServer.Authority.ToString()) },
            ScopesSupported = ["mcp:tools"],
        };
    });


services.AddAuthorization();

services    
    .AddMcpServer()
    .WithHttpTransport()
    //.WithPrompts<MonkeyPrompts>()
    //.WithResources<MonkeyResources>()
    .WithTools<MonkeyTools>();

services.AddHttpClient()
    .AddSingleton<MonkeyService>()
    .AddSingleton<MonkeyLocationService>();

var app = builder.Build();

app
    .UseAuthentication()
    .UseAuthorization();

app
    .MapMcp()
    .RequireAuthorization();

app.Run();