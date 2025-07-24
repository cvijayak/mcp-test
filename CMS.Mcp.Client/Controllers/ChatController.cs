namespace CMS.Mcp.Client.Controllers 
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Models;
    using Contracts.Options;
    using Contracts.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    [Authorize]
    [Route("mcp/chat")]
    public class ChatController(IChatMessageStore chatMessageStore,
        IAiAssistantService aiAssistantService,
        Func<string, IMcpToolService> mcpToolServiceFactory,
        IOptions<ServerOptions> serverOptions,
        ILogger<ChatController> logger) : Controller
    {
        private readonly ServerOptions _serverOptions = serverOptions.Value;

        private IMcpToolService GetMcpToolService(string serverName)
        {
            var nameOfServer = string.IsNullOrEmpty(serverName) ? _serverOptions.McpServers?.FirstOrDefault(s => s.Name == serverName)?.Name : serverName;
            return mcpToolServiceFactory(nameOfServer);
        }

        [Authorize]
        [Route("")]
        public IActionResult Index()
        {
            var servers = _serverOptions.McpServers?.Select(s => s.Name).ToArray() ?? Array.Empty<string>();
            var isAuthenticated = User.Identity?.IsAuthenticated == true;

            var model = new TokenInfoViewModel
            {
                AccessToken = isAuthenticated ? "authenticated" : string.Empty,
                ApiBasePath = Url.Content("~/"),
                McpClientBasePath = Url.Content("~/"),
                ServerOptions = servers
            };

            return View(model);
        }

        [HttpPost]
        [Route("SendMessage")]
        public async Task<IActionResult> SendMessage(string message, string serverName = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest("Message cannot be empty");
            }

            try
            {
                var toolService = GetMcpToolService(serverName);
                await toolService.RegisterToolsAsync();

                var response = await aiAssistantService.SendMessageAsync(message, serverName);
                return Json(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending message");
                return StatusCode(500, "An error occurred while sending the message");
            }
        }

        [HttpPost]
        [Route("ClearChat")]
        public async Task<IActionResult> ClearChat()
        {
            await aiAssistantService.ClearChatAsync();
            return Json(new { success = true, message = "Chat cleared successfully" });
        }

        [HttpGet]
        [Authorize]
        [Route("GetMessages")]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await chatMessageStore.ListAsync();
            return Json(messages);
        }

        [HttpGet]
        [Authorize]
        [Route("GetMcpTools")]
        public async Task<IActionResult> GetMcpTools(string serverName = null)
        {
            var availableServers = _serverOptions.McpServers?.Select(s => s.Name).ToArray() ?? Array.Empty<string>();
            var toolService = GetMcpToolService(serverName);
            var tools = await toolService.GetToolsAsync();
            
            ViewBag.ServerOptions = availableServers;
            ViewBag.SelectedServer = serverName;
            
            return View(tools);
        }

        [HttpPost]
        [Authorize]
        [Route("ExecuteTool")]
        public async Task<IActionResult> ExecuteTool([FromBody] ExecuteToolRequest request)
        {
            var toolNameToUse = request?.ToolName;
            var serverNameToUse = request?.ServerName;
            var paramDict = request?.Parameters ?? new Dictionary<string, object>();
            
            if (string.IsNullOrEmpty(toolNameToUse))
            {
                return BadRequest("Tool name is required");
            }
            
            var toolService = GetMcpToolService(serverNameToUse);
            var result = await toolService.ExecuteToolAsync(toolNameToUse, paramDict.ToDictionary(x => x.Key, x => x.Value));
            return Json(result);
        }
        
        //[HttpPost]
        //[Authorize]
        //[Route("ExecuteToolForm")]
        //public async Task<IActionResult> ExecuteToolForm(string toolName, string serverName, [FromForm] Dictionary<string, string> parameters)
        //{
        //    if (string.IsNullOrEmpty(toolName))
        //    {
        //        return BadRequest("Tool name is required");
        //    }
            
        //    var paramDict = new Dictionary<string, object>();
        //    foreach (var param in parameters)
        //    {
        //        if (param.Key != "toolName" && param.Key != "serverName" && !string.IsNullOrEmpty(param.Value))
        //        {
        //            paramDict[param.Key] = param.Value;
        //        }
        //    }
            
        //    var toolService = GetMcpToolService(serverName);
        //    var result = await toolService.ExecuteToolAsync(toolName, paramDict);
            
        //    TempData["ToolResult"] = result?.ToString() ?? "Tool executed successfully";
        //    return RedirectToAction("GetMcpTools", new { serverName = serverName });
        //}
        
        [HttpGet]
        [AllowAnonymous]
        [Route("GetMcpServers")]
        public IActionResult GetMcpServers()
        {
            try
            {
                var servers = _serverOptions?.McpServers?.Select(s => s.Name).ToArray() ?? Array.Empty<string>();
                return Json(servers);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting MCP servers");
                return Json(Array.Empty<string>());
            }
        }
    }
}
