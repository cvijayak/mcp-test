namespace CMS.Mcp.Client.Extensions
{
    using System;
    using System.IO;
    using System.Net.Http;
    using Contracts;
    using Contracts.Options;
    using Contracts.Providers;
    using Contracts.Services;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.SemanticKernel;
    using Security;
    using Security.Extensions;
    using Security.Providers;
    using Services;
    using Shared.Api.Clients;
    using Shared.Api.Clients.Caches;
    using Shared.Api.Clients.Contracts;
    using Shared.Api.Clients.Contracts.Caches;
    using Shared.Api.Clients.Contracts.Configs;
    using Shared.Api.Clients.Contracts.Params;
    using Shared.Common;
    using DataProtectionOptions = Contracts.Options.DataProtectionOptions;

    public static class ServiceCollectionX
    {
        public static IServiceCollection AddClientServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddTransient(_ => JsonObjectSerializerFactory.Create())
                .AddMemoryCache()
                .AddOptions(configuration)
                .AddHttpContextAccessor()
                .AddCors(configuration)
                .AddProviders()
                .AddSecurity(configuration)
                .AddHttpClients(configuration)
                .AddApiClients(configuration)
                .AddMcp()
                .AddOptions();
        }

        private static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            var identityServerSection = configuration.GetSection(ConfigSectionNames.IDENTITY_SERVER);
            services.Configure<IdentityServerOptions>(identityServerSection);

            var serverSection = configuration.GetSection(ConfigSectionNames.SERVER);
            services.Configure<ServerOptions>(serverSection);

            var azureOpenAiChatSection = configuration.GetSection(ConfigSectionNames.AZURE_OPEN_AI_CHAT);
            services.Configure<AzureOpenAIChatOptions>(azureOpenAiChatSection);

            return services;
        }

        public static IServiceCollection AddMcpClient(this IServiceCollection services)
        {
            services.AddSingleton<IChatService, ChatService>();
            services.AddHttpClient();
            return services;
        }

        private static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
        {
            var corsAllowedOriginsSection = configuration.GetSection(ConfigSectionNames.CORS_ALLOWED_ORIGINS);
            var corsAllowedOrigins = corsAllowedOriginsSection.Get<string[]>();

            return services.AddCors(cors =>
            {
                cors.AddDefaultPolicy(policy => policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins(corsAllowedOrigins)
                    .AllowCredentials());
            });
        }

        private static IServiceCollection AddProviders(this IServiceCollection services)
        {
            return services
                .AddScoped<IClaimStore, ClaimStore>()
                .AddTransient<ISessionProvider, SessionProvider>();
        }

        private static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            var dataProtectionSection = configuration.GetSection(ConfigSectionNames.DATA_PROTECTION);
            var dataProtection = dataProtectionSection.Get<DataProtectionOptions>();

            var identityServerSection = configuration.GetSection(ConfigSectionNames.IDENTITY_SERVER);
            var identityServer = identityServerSection.Get<IdentityServerOptions>();

            services
                .AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dataProtection.Directory))
                .SetApplicationName(dataProtection.ApplicationName);

            services
                .AddTransient<ClaimsTransformer>()
                .AddTransient<TokenSessionManager>()
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = AuthSchemeNames.AUTHENTICATION_OIDC_SCHEME_NAME;
                })
                .AddCookie(o => o.ConfigureCookieAuthenticationScheme())
                .AddOAuth(AuthSchemeNames.AUTHENTICATION_OIDC_SCHEME_NAME, o => o.ConfigureOAuthAuthenticationScheme(identityServer));

            services
                .AddAuthorization()
                .AddMvcCore();

            return services;
        }

        private static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            var identityServerSection = configuration.GetSection(ConfigSectionNames.IDENTITY_SERVER);
            var identityServer = identityServerSection.Get<IdentityServerOptions>();

            services.AddHttpClient(HttpClientInstances.IDENTITY_SERVER_HTTP_CLIENT_INSTANCE, client => client.BaseAddress = identityServer.Authority);

            return services;
        }

        private static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
        {
            var identityServerSection = configuration.GetSection(ConfigSectionNames.IDENTITY_SERVER);
            var identityServer = identityServerSection.Get<IdentityServerOptions>();

            return services    
                .AddSingleton<IClientResponseCache, ClientResponseCache>()
                .AddTransient(sp => new Func<RestClientParams, IRestClient>(c => ActivatorUtilities.CreateInstance<RestClient>(sp, c)))
                .AddTransient<IIdentityApiClient>(sp =>
                {
                    var c = new IdentityClientParams
                    {
                        RestClientParams = new RestClientParams
                        {
                            HttpClientInstance = HttpClientInstances.IDENTITY_SERVER_HTTP_CLIENT_INSTANCE,
                            ExternalServerType = ExternalServerType.IDENTITY_SERVER,
                            ConfigureHttpClient = _ => { }
                        },
                        OidcEndpointConfig = new OidcEndpointConfig
                        {
                            ResponseCacheTimeout = identityServer.OidcEndpoint.ResponseCacheTimeout
                        },
                        ClientCredentialsConfig = new ClientCredentialsConfig
                        {
                            ClientId = identityServer.AuthorizationCode.ClientId,
                            ClientSecret = identityServer.AuthorizationCode.ClientSecret,
                            ResponseCacheTimeout = identityServer.AuthorizationCode.ResponseCacheTimeout
                        },
                        Scope = identityServer.AuthorizationCode.Scopes
                    };

                    return ActivatorUtilities.CreateInstance<IdentityApiClient>(sp, c);
                });
        }

        private static IServiceCollection AddMcp(this IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                var azureOpenAiChatOptions = sp.GetRequiredService<IOptions<AzureOpenAIChatOptions>>().Value;

                var deploymentName = azureOpenAiChatOptions.DeploymentName;
                var endpoint = azureOpenAiChatOptions.Endpoint.ToString();
                var apiKey = azureOpenAiChatOptions.ApiKey;
                var handler = new HttpClientHandler
                {
                    SslProtocols = System.Security.Authentication.SslProtocols.Tls12 |
                                   System.Security.Authentication.SslProtocols.Tls13,
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                var httpClient = new HttpClient(handler);

                return Kernel
                    .CreateBuilder()
                    .AddAzureOpenAIChatCompletion(deploymentName: deploymentName, endpoint: endpoint, apiKey: apiKey, httpClient: httpClient)
                    .Build();
            });

            return services.AddSingleton<IMcpClientProvider, McpClientProvider>();
        }
    }
}