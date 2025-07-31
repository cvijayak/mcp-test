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

    public class AssistantService(IChatMessageStore chatMessageStore,
        ISummaryService summaryService,
        Func<string, IMcpToolService> mcpToolServiceFactory,
        Func<string, Kernel> kernelFactory,
        ILogger<AssistantService> logger) : IAssistantService
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

                var mcpToolService = mcpToolServiceFactory(serverName);
                var tools = await mcpToolService.ListAsync(cancellationToken);
                
                var serverContextBuilder = new StringBuilder();
                serverContextBuilder.AppendLine($"MCP Server: {serverName}");
                serverContextBuilder.AppendLine("Available Tools:");
                
                if (tools.Any())
                {
                    foreach (var tool in tools)
                    {
                        serverContextBuilder.AppendLine($"- {tool.Name}: {tool.Description}");
                    }
                }
                else
                {
                    serverContextBuilder.AppendLine("- No tools information available for this server");
                }
                
                string serverContext = serverContextBuilder.ToString();
                string conversationSummary = summaryService.Get();

                string prompt = $"""
                                 You are a technical assistant specializing in systems that use MCP servers.

                                 Your task is to generate a relevant and context-aware response to the user's latest message.

                                 Please follow these important instructions:
                                 - Consider both the **MCP Server Context** and the **Conversation Summary** when crafting your response.
                                 - Use the **User Message** as the primary prompt for your reply.
                                 - Your response should be accurate, technically sound, and helpful.

                                 === MCP SERVER CONTEXT ===
                                 {serverContext}

                                 === CONVERSATION SUMMARY ===
                                 {conversationSummary}

                                 === USER MESSAGE ===
                                 {message}

                                 === ASSISTANT RESPONSE ===
                                 """;

#pragma warning disable SKEXP0001
                var options = new FunctionChoiceBehaviorOptions { RetainArgumentTypes = true };
                var promptExecutionSettings = new OpenAIPromptExecutionSettings
                {
                    Temperature = 0.3,
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: options)
                };
                var kernelArguments = new KernelArguments(promptExecutionSettings);
                var result = await kernel.InvokePromptAsync(prompt, kernelArguments, cancellationToken: cancellationToken);
#pragma warning restore SKEXP0001

                assistantMessage.Content = result.GetValue<string>();

                logger.LogInformation($"Successfully generated response: {assistantMessage.Content[..Math.Min(assistantMessage.Content.Length, 50)]}...");

                assistantMessage.IsProcessing = false;

                await summaryService.SummarizeAsync(kernel);

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

        public void ClearMessages()
        {
            chatMessageStore.Clear();
        }
    }
}