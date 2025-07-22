# Model Context Protocol (MCP) Client

This project implements a client-side application for interacting with Model Context Protocol (MCP) services. The solution provides tools for executing AI-powered capabilities through a clean, web-based interface.

## Overview

The MCP Client is built as a .NET web application that communicates with AI services through the Model Context Protocol. It includes a user interface for browsing available AI tools, executing them with parameters, and viewing the results in different formats.

## Project Structure

The solution consists of several projects:

- **CMS.Mcp.Client.Host**: The web application host that serves the UI and handles HTTP requests
- **CMS.Mcp.Client**: Core client functionality for communicating with MCP services
- **CMS.Mcp.Client.Contracts**: Interfaces and models defining the client API
- **CMS.Mcp.Client.Security**: Authentication and authorization components
- **CMS.Mcp.Server**: Server-side implementation for hosting MCP services
- **CMS.Mcp.Server.Contracts**: Server-side interfaces and models
- **CMS.Mcp.Shared**: Shared components used by both client and server implementations
  - **Api.Clients**: HTTP client implementations for external APIs
  - **Common**: Utility classes and extensions

## Features

### MCP Tools Interface

The application provides a web interface for:

- Viewing available AI tools with descriptions
- Configuring tool parameters through a dynamic form interface
- Executing tools and viewing results in various formats:
  - JSON view
  - Tree view (hierarchical visualization)
  - Table view (for array data)
  - Image detection and display (for URLs that point to images)

### SSE-based Communication

The client uses Server-Sent Events (SSE) for real-time communication with the MCP server:

- **McpSseTransport**: Handles the SSE-based connection to the MCP service
- **McpClientProvider**: Creates and manages MCP client instances

### Security

Authentication is handled through JWT tokens:

- **ClaimStore**: Manages user claims and permissions
- **TokenSessionManager**: Manages token lifecycle including renewal

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or later (recommended)

### Running the Application

1. Clone this repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Build the solution
5. Set CMS.Mcp.Client.Host as the startup project
6. Run the application

### Configuration

The application is configured through appsettings.json files in both Client.Host and Server projects. Key configuration sections include:

- **ServerOptions**: Configures the MCP server endpoint
- **IdentityServerOptions**: Configures authentication

## Development

### Adding New MCP Tools

1. Server-side: Implement new tools in the CMS.Mcp.Server project
2. Tools are automatically discovered and exposed through the MCP interface
3. Parameters defined in the tool schema are used to generate UI elements

### Key Components

#### McpToolViewModel

This model represents an AI tool in the UI:

```csharp
public class McpToolViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Parameters { get; set; } = Array.Empty<string>();
}
```

#### ChatService

The ChatService communicates with the MCP client to:

- List available tools
- Execute tools with parameters
- Handle chat messages

## Build Notes

If you encounter StaticWebAssets compression errors during build, the project includes special handling in the CMS.Mcp.Client.Host.csproj file to disable problematic compression features.