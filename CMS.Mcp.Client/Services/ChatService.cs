namespace CMS.Mcp.Client.Services 
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Models;
    using Contracts.Services;
    using Microsoft.Extensions.Logging;
    using NJsonSchema;
    using Shared.Common.Extensions;

    public class ChatService(IMcpClientProxy clientProxy, ILogger<ChatService> logger) : IChatService
    {
        public List<ChatMessageViewModel> Messages { get; } = [];

        public async Task<McpToolViewModel[]> GetToolsAsync()
        {
            var result = await clientProxy.ListToolsAsync();
            return await Task.WhenAll(result.Select(async d =>
            {
                var schema = await JsonSchema.FromJsonAsync(JsonSerializer.Serialize(d.ProtocolTool.InputSchema));
                var properties = schema.Properties.Select(p => new McpToolViewModel.McpToolParameter
                {
                    Name = p.Value.Name,
                    Description = p.Value.Description,
                    Type = p.Value.Type.ToString()
                }).ToArray();

                return new McpToolViewModel
                {
                    Title = d.Title,
                    Name = d.Name,
                    Description = d.Description,
                    Parameters = properties
                };
            }));
        }

        public async Task<JsonNode> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters)
        {
            var result = await clientProxy.CallToolAsync(toolName, parameters);
            var textResult = (string)((dynamic)result.Content.FirstOrDefault())?.Text ?? string.Empty;
            var textResultIsValidJson = textResult.IsValidJson();
            var text = result.IsError == true ? JsonSerializer.Serialize(new { error = textResult }) 
                : (textResultIsValidJson ? textResult : JsonSerializer.Serialize(new { message = textResult }));

            return JsonNode.Parse(text);
        }

        public async Task<ChatMessageViewModel> SendMessageAsync(string message)
        {
            var userMessage = new ChatMessageViewModel 
            {
                Content = message,
                Role = ChatRole.User,
                Timestamp = DateTime.UtcNow
            };
            Messages.Add(userMessage);

            var assistantMessage = new ChatMessageViewModel 
            {
                Content = "...",
                Role = ChatRole.Assistant,
                IsProcessing = true,
                Timestamp = DateTime.UtcNow
            };
            Messages.Add(assistantMessage);

            try 
            {
                logger.LogInformation($"Processing message: {message}");

                var result = await clientProxy.InvokePromptAsync(message);
                assistantMessage.Content = result.GetValue<string>();

                logger.LogInformation($"Successfully generated response: {assistantMessage.Content[..Math.Min(assistantMessage.Content.Length, 50)]}...");

                assistantMessage.IsProcessing = false;
                return assistantMessage;
            } 
            catch (Exception ex) 
            {
                assistantMessage.Content = $"Error: {ex.Message}";
                assistantMessage.IsProcessing = false;
                logger.LogError(ex, "Error sending message");
                return assistantMessage;
            }
        }
        
        public Task ClearChatAsync()
        {
            Messages.Clear();
            return Task.CompletedTask;
        }
    }
}
