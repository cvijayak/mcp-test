namespace CMS.Mcp.Client.Controllers 
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Models;
    using Contracts.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [Route("mcp/chat")]
    public class ChatController(IChatMessageStore chatMessageStore, 
        IAiAssistantService aiAssistantService, 
        Func<string, IMcpToolService> mcpToolServiceFactory, 
        ILogger<ChatController> logger) : Controller
    {
        [Authorize]
        [Route("")]
        public IActionResult Index()
        {
            var model = new TokenInfoViewModel
            {
                AccessToken = User.Identity?.IsAuthenticated == true ? "authenticated" : string.Empty,
                ApiBasePath = Url.Content("~/"),
                McpClientBasePath = Url.Content("~/")
            };

            return View(model);
        }

        [HttpPost]
        [Route("SendMessage")]
        public async Task<IActionResult> SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest("Message cannot be empty");
            }

            try
            {
                var toolService = mcpToolServiceFactory("MonkeyMcpClientTool");
                await toolService.RegisterToolsAsync();

                var response = await aiAssistantService.SendMessageAsync(message);
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
        public IActionResult GetMessages()
        {
            var messages = chatMessageStore.List();
            logger.LogInformation($"GetMessages returning {messages.Length} messages");
            return Json(messages);
        }

        [HttpGet]
        [Authorize]
        [Route("GetMcpTools")]
        public async Task<IActionResult> GetMcpTools()
        {
            var toolService = mcpToolServiceFactory("MonkeyMcpClientTool");
            var tools = await toolService.GetToolsAsync();
            return View(tools);
        }

        [HttpPost]
        [Authorize]
        [Route("ExecuteTool")]
        public async Task<IActionResult> ExecuteTool([FromBody] ExecuteToolRequest request)
        {
            var paramDict = request?.Parameters ?? new Dictionary<string, object>();
            var toolService = mcpToolServiceFactory("MonkeyMcpClientTool");
            var result = await toolService.ExecuteToolAsync(request.ToolName, paramDict.ToDictionary(x => x.Key, x => x.Value));
            return Json(result);
        }
    }
}
