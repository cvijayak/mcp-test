namespace CMS.Mcp.Client.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Options;
    using Contracts.Services;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.SemanticKernel;
    using ModelContextProtocol.Client;
    using Security;
    using Security.Contracts;
    using Security.Contracts.Providers;
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
        public static IServiceCollection AddMcpAgent(this IServiceCollection services, IConfiguration configuration)
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
                .AddSemanticKernel()
                .AddMcp()
                .AddOptions()
                .AddStores()
                .AddServices();
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

        private static IServiceCollection AddStores(this IServiceCollection services)
        {
            return services.AddSingleton<IChatMessageStore, ChatMessageStore>();
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IAiAssistantService, AiAssistantService>()
                .AddTransient<Func<string, IMcpToolService>>(sp => n => ActivatorUtilities.CreateInstance<McpToolService>(sp, n));
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
                .AddTransient<IClaimStore, ClaimStore>()
                .AddTransient<IClaimStoreProvider, ClaimStoreProvider>()
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

            services
                .AddHttpClient(HttpClientInstances.IDENTITY_SERVER_HTTP_CLIENT_INSTANCE, client => client.BaseAddress = identityServer.Authority);

            services
                .AddHttpClient(HttpClientInstances.AZURE_OPEN_AI_CHAT_HTTP_CLIENT_INSTANCE, _ => { })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    SslProtocols = System.Security.Authentication.SslProtocols.Tls12 |
                                   System.Security.Authentication.SslProtocols.Tls13,
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

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

        private static IServiceCollection AddSemanticKernel(this IServiceCollection services)
        {
            return services.AddSingleton<Func<string, Kernel>>(sp =>
            {
                var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
                var azureOpenAiChatOptions = sp.GetRequiredService<IOptions<AzureOpenAIChatOptions>>().Value;
                var cache = new ConcurrentDictionary<string, Kernel>();

                return serverName =>
                {
                    if (string.IsNullOrEmpty(serverName))
                    {
                        return null;
                    }

                    if (cache.TryGetValue(serverName, out var kernel))
                    {
                        return kernel;
                    }

                    var deploymentName = azureOpenAiChatOptions.DeploymentName;
                    var endpoint = azureOpenAiChatOptions.Endpoint.ToString();
                    var apiKey = azureOpenAiChatOptions.ApiKey;
                    var httpClient = httpFactory.CreateClient(HttpClientInstances.AZURE_OPEN_AI_CHAT_HTTP_CLIENT_INSTANCE);

#pragma warning disable SKEXP0001
                    kernel = Kernel
                        .CreateBuilder()
                        .AddAzureOpenAIChatCompletion(deploymentName: deploymentName, endpoint: endpoint, apiKey: apiKey, httpClient: httpClient)
                        .Build();
#pragma warning restore SKEXP0001

                    cache[serverName] = kernel;

                    return kernel;
                };
            });
        }

        private static IServiceCollection AddMcp(this IServiceCollection services)
        {
            return services
                .AddSingleton<Func<McpServerConfig, CancellationToken, Task<IMcpClient>>>(sp =>
                {
                    var cache = new ConcurrentDictionary<string, IMcpClient>();
                    return async (config, cToken) =>
                    {
                        if (config == null || string.IsNullOrEmpty(config.Name) || string.IsNullOrEmpty(config.BaseUrl))
                        {
                            return null;
                        }

                        var endpoint = config.GetSseUri();
                        var name = config.Name;
                        var key = $"{endpoint}:::{name}";

                        if (cache.TryGetValue(key, out var mcpClient))
                        {
                            return mcpClient;
                        }

                        var transport = ActivatorUtilities.CreateInstance<McpSseTransport>(sp, endpoint, name);
                        mcpClient = await McpClientFactory.CreateAsync(transport, cancellationToken: cToken);
                        cache[key] = mcpClient;

                        return mcpClient;
                    };
                });
        }
    }
}