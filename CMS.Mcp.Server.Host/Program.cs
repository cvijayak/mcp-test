using CMS.Mcp.Server.Extensions;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

services.AddMcpServer(configuration);

var app = builder.Build();

app
    .UseMcpServer();

app
    .MapMcp()
    .RequireAuthorization();

app.Run();