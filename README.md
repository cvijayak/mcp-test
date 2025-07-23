# Model Context Protocol (MCP) Client

This project implements a client-side application for interacting with Model Context Protocol (MCP) services. The solution provides tools for executing AI-powered capabilities through a clean, web-based interface.

---

## Purpose

The MCP Client is designed to:
- Provide a user-friendly web interface for interacting with AI tools and services via the Model Context Protocol.
- Allow users to browse, configure, and execute AI tools with parameters.
- Display results in rich formats, including support for HTML and code blocks.
- Serve as a reference implementation for integrating Semantic Kernel and OpenAI/Azure OpenAI with .NET web applications.

---

## Technologies Used

- **.NET 9.0**: Core framework for backend and web application.
- **ASP.NET Core MVC**: For building the web UI and RESTful endpoints.
- **Semantic Kernel**: For orchestrating AI skills and tool execution.
- **OpenAI / Azure OpenAI**: For AI-powered chat and tool responses.
- **Bootstrap 5**: For responsive and modern UI components.
- **FontAwesome**: For iconography.
- **JavaScript (ES6+)**: For client-side interactivity.
- **DOMPurify**: For safe HTML rendering in chat.
- **Razor Views**: For dynamic server-side rendering.

---

## NuGet Packages Used

- `Microsoft.SemanticKernel`  
  *Purpose*: Provides orchestration and integration with AI models and skills.

- `Microsoft.SemanticKernel.Agents.Core`  
  *Purpose*: (If present) Used for advanced agent orchestration with Semantic Kernel.

- `Microsoft.AspNetCore.Mvc`  
  *Purpose*: ASP.NET Core MVC framework for controllers and views.

- `Microsoft.Extensions.Logging`  
  *Purpose*: Logging infrastructure for .NET applications.

- `System.Text.Json`  
  *Purpose*: JSON serialization/deserialization.

- `Microsoft.IdentityModel.*`  
  *Purpose*: Authentication and authorization.

- `Microsoft.OpenApi.*`  
  *Purpose*: Swagger/OpenAPI support for API documentation.

- `Newtonsoft.Json`  
  *Purpose*: (If present) Advanced JSON handling.

> **Note:** Some packages may be referenced transitively or via project dependencies.

---

## How to Run the Project

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Node.js (for front-end asset builds, if required)
- Access to OpenAI or Azure OpenAI API (for AI chat/tools)
- (Optional) Visual Studio 2022+ or VS Code

### Steps

1. **Clone the repository**
   ```sh
   git clone <your-repo-url>
   cd <repo-folder>
   ```

2. **Restore NuGet packages**
   ```sh
   dotnet restore
   ```

3. **Configure settings**
   - Update `appsettings.json` or user secrets with your OpenAI/Azure OpenAI keys and endpoints.
   - Example:
     ```json
     {
       "OpenAI": {
         "Key": "YOUR_API_KEY",
         "Endpoint": "https://api.openai.com/v1/"
       }
     }
     ```

4. **Build the solution**
   ```sh
   dotnet build
   ```

5. **Run the web application**
   ```sh
   dotnet run --project CMS.Mcp.Client.Host
   ```
   The app will be available at `https://localhost:5001/mcp` by default.

6. **Access the UI**
   - Open your browser and navigate to `https://localhost:5001/mcp/chat` for the chat interface.
   - Go to `https://localhost:5001/mcp/chat/GetMcpTools` for the MCP Tools interface.

---

## Project Structure

- **CMS.Mcp.Client.Host**: The web application host that serves the UI and handles HTTP requests.
- **CMS.Mcp.Client**: Core client functionality for communicating with MCP services and orchestrating AI tools.
- **CMS.Mcp.Client.Contracts**: Interfaces and models defining the client API and data contracts.
- **CMS.Mcp.Client.Security**: Authentication and authorization components.
- **CMS.Mcp.Server**: (If present) Server-side implementation for hosting MCP services.
- **CMS.Mcp.Server.Contracts**: Server-side interfaces and models.

---

## Additional Notes

- If you encounter StaticWebAssets compression errors during build, the project includes special handling in the CMS.Mcp.Client.Host.csproj file to disable problematic compression features.
- The chat interface supports both plain text and HTML/rich content, with sanitization for security.
- The MCP Tools page allows parameterized tool execution with collapsible parameter sections.

---