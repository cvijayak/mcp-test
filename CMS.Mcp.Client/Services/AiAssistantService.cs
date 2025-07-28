namespace CMS.Mcp.Client.Services
{
    using System;
    using System.Linq;
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

        public async Task<string[]> GetSuggestionsAsync(string serverName, CancellationToken cancellationToken)
        {
            string suggestionsPrompt = $"""
                                        Based on the summary and assistant's reply below, suggest three concise follow-up questions or clarifications. Return empty if summary or assistant's reply is empty:

                                        Summary:
                                        {summaryStore.Get()}
                                        
                                        Assistant said:
                                        {chatMessageStore.LastOrDefault()?.Content ?? string.Empty} 
                                                                 
                                        Suggestions:
                                        1.
                                        2.
                                        3.
                                        """;

            var kernel = kernelFactory(serverName);
            if (kernel == null)
            {
                return [];
            }

            var executionSettings = new OpenAIPromptExecutionSettings { Temperature = 0.3 };
            var arguments = new KernelArguments(executionSettings);
            var suggestionsResponse = await kernel.InvokePromptAsync(suggestionsPrompt, arguments, cancellationToken: cancellationToken);

            var suggestions = suggestionsResponse.GetValue<string>();

            return suggestions
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line =>
                {
                    int dotIndex = line.IndexOf('.');
                    if (dotIndex >= 0 && dotIndex + 1 < line.Length)
                    {
                        return line[(dotIndex + 1)..].Trim();
                    }

                    return line.Trim();
                })
                .ToArray();
        }

        public void ClearChat()
        {
            chatMessageStore.Clear();
        }
    }
}
                                                                                   