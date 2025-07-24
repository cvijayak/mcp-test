namespace CMS.Mcp.Client.Services
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Models;
    using Contracts.Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.Connectors.OpenAI;

    public class AiAssistantService(IChatMessageStore chatMessageStore, Func<string, Kernel> kernelFactory, ILogger<AiAssistantService> logger) : IAiAssistantService
    {
        private async Task<ChatMessageViewModel> AddChatMessageAsync(string message)
        {
            var userMessage = new ChatMessageViewModel 
            {
                Content = message,
                Role = ChatRole.User,
                Timestamp = DateTime.UtcNow
            };

            var assistantMessage = new ChatMessageViewModel 
            {
                Content = "...",
                Role = ChatRole.Assistant,
                IsProcessing = true,
                Timestamp = DateTime.UtcNow
            };
            
            await chatMessageStore.AddAsync(userMessage);
            await chatMessageStore.AddAsync(assistantMessage);

            return assistantMessage;
        }

        public async Task<ChatMessageViewModel> SendMessageAsync(string message, string serverName)
        {
            var assistantMessage = await AddChatMessageAsync(message);

            try 
            {
                if (string.IsNullOrEmpty(serverName))
                {
                    assistantMessage.Content = "No MCP server selected or available. Please check your configuration.";
                    assistantMessage.IsProcessing = false;
                    return assistantMessage;
                }
                
                logger.LogInformation($"Processing message: {message} with server: {serverName}");
                
                var kernel = kernelFactory(serverName);
                if (kernel == null)
                {
                    assistantMessage.Content = "Unable to find the McpServer configuration";
                    assistantMessage.IsProcessing = false;
                    return assistantMessage;
                }

#pragma warning disable SKEXP0001
                var result = await kernel.InvokePromptAsync(message, new KernelArguments(new OpenAIPromptExecutionSettings
                {
                    Temperature = 0,
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
                }));
#pragma warning restore SKEXP0001

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

        public async Task ClearChatAsync()
        {
            await chatMessageStore.ClearAsync();
        }
    }
}
