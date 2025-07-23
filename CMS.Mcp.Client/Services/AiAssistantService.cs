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

    public class AiAssistantService(IChatMessageStore chatMessageStore, Kernel kernel, ILogger<AiAssistantService> logger) : IAiAssistantService
    {
        public async Task<ChatMessageViewModel> SendMessageAsync(string message)
        {
            var userMessage = new ChatMessageViewModel 
            {
                Content = message,
                Role = ChatRole.User,
                Timestamp = DateTime.UtcNow
            };
            chatMessageStore.Add(userMessage);

            var assistantMessage = new ChatMessageViewModel 
            {
                Content = "...",
                Role = ChatRole.Assistant,
                IsProcessing = true,
                Timestamp = DateTime.UtcNow
            };
            chatMessageStore.Add(assistantMessage);

            try 
            {
                logger.LogInformation($"Processing message: {message}");

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
        
        public Task ClearChatAsync()
        {
            chatMessageStore.Clear();
            return Task.CompletedTask;
        }
    }
}
