namespace CMS.Mcp.Client.Services
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Models;
    using Contracts.Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.Connectors.OpenAI;

    public class AiAssistantService(IChatMessageStore chatMessageStore, ISummaryStore summaryStore, Func<string, Kernel> kernelFactory, ILogger<AiAssistantService> logger) : IAiAssistantService
    {
        private ChatMessageViewModel AddChatMessage(string message)
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
            
            chatMessageStore.Add(userMessage); 
            chatMessageStore.Add(assistantMessage);

            return assistantMessage;
        }

        private async Task SummarizeAsync(Kernel kernel)
        {
            var buffer = new StringBuilder();
            foreach (var message in chatMessageStore.List())
            {
                buffer.AppendLine(message.Role == ChatRole.User ? $"User: {message.Content}" : (!message.IsProcessing ? $"Assistant: {message.Content}" : string.Empty));
            }

            string fullHistory = buffer.ToString();
            string summaryPrompt = $"Summarize the following conversation history briefly for context:\n{fullHistory}\nSummary:";

            var promptExecutionSettings = new OpenAIPromptExecutionSettings { Temperature = 0 };
            var kernelArguments = new KernelArguments(promptExecutionSettings);
            var summaryResult = await kernel.InvokePromptAsync(summaryPrompt, kernelArguments);

            summaryStore.Add(summaryResult.GetValue<string>());
        }

        public async Task<ChatMessageViewModel> SendMessageAsync(string message, string serverName, CancellationToken cancellationToken)
        {
            var assistantMessage = AddChatMessage(message);

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

                string prompt = $"Summary:{summaryStore.Get()}\nUser: {message}\nAssistant:";

#pragma warning disable SKEXP0001
                var options = new FunctionChoiceBehaviorOptions { RetainArgumentTypes = true };
                var promptExecutionSettings = new OpenAIPromptExecutionSettings
                {
                    Temperature = 0,
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: options)
                };
                var kernelArguments = new KernelArguments(promptExecutionSettings);
                var result = await kernel.InvokePromptAsync(prompt, kernelArguments, cancellationToken: cancellationToken);
#pragma warning restore SKEXP0001

                assistantMessage.Content = result.GetValue<string>();

                logger.LogInformation($"Successfully generated response: {assistantMessage.Content[..Math.Min(assistantMessage.Content.Length, 50)]}...");

                assistantMessage.IsProcessing = false;

                await SummarizeAsync(kernel);

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

        public void ClearChat()
        {
            chatMessageStore.Clear();
        }
    }
}
