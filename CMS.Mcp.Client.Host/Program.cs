using System;
using CMS.Mcp.Client.Contracts;
using CMS.Mcp.Client.Extensions;
using CMS.Mcp.Shared.Common;
using CMS.Mcp.Shared.Common.Contracts;
using CMS.Mcp.Shared.Configuration.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

ResourceRegistry.Instance.RegisterAssembly(typeof(Program).Assembly, [ typeof(EncryptionKey) ] );

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var healthCheckUrlSection = configuration.GetSection(ConfigSectionNames.HEALTH_CHECK_URL);
var healthCheckUrl = healthCheckUrlSection.Get<string>();

builder.Configuration.AddConfigurationSource();

builder.Logging
    .ClearProviders()
    .AddConfiguration(configuration.GetSection("Logging"))
    .AddConsole()
    .AddDebug();

builder.Services
    .AddResponseCompression(options => options.EnableForHttps = true)
    .AddClientServices(configuration)
    .AddControllersWithViews();

builder.Services.AddHealthChecks();

builder.Services.AddMcpClient();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
app.Lifetime.ApplicationStarted.Register(() => logger.LogInformation("Started McpChatClient"));
app.Lifetime.ApplicationStopping.Register(() => logger.LogInformation("Stopping McpChatClient"));
app.Lifetime.ApplicationStopped.Register(() => logger.LogInformation("Stopped McpChatClient successfully"));

if (app.Environment.IsDevelopment()) 
{
    app.UseDeveloperExceptionPage();
}

var contentTypeProvider = new FileExtensionContentTypeProvider();

app
    .UseResponseCompression()
    .UseStaticFiles(new StaticFileOptions
    {
        ContentTypeProvider = contentTypeProvider,
        OnPrepareResponse = ctx => ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600")
    })
    .UseStaticFiles(new StaticFileOptions
    {
        RequestPath = "/mcp",
        ContentTypeProvider = contentTypeProvider,
        OnPrepareResponse = ctx => ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600")
    })
    .UseMcpClient()
    .UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto })
    .UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.None });

app.MapControllers();
app.MapHealthChecks(healthCheckUrl);


try
{
    app.Run();
}
catch (Exception exception)
{
    logger.LogError(exception, "Unable to stop Engine gracefully due to exception");
}