using System;
using CMS.Mcp.Client.Contracts;
using CMS.Mcp.Client.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

var healthCheckUrlSection = configuration.GetSection(ConfigSectionNames.HEALTH_CHECK_URL);
var healthCheckUrl = healthCheckUrlSection.Get<string>();

services
    .AddMcpAgent(configuration)
    .AddControllersWithViews();

services.AddHealthChecks();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
app.Lifetime.ApplicationStarted.Register(() => logger.LogInformation("Started McpAgent"));
app.Lifetime.ApplicationStopping.Register(() => logger.LogInformation("Stopping McpAgent"));
app.Lifetime.ApplicationStopped.Register(() => logger.LogInformation("Stopped McpAgent successfully"));

if (app.Environment.IsDevelopment()) 
{
    app.UseDeveloperExceptionPage();
}

app.UseMcpAgent();

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