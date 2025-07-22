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
    using ModelContextProtocol.Client;
    using Shared.Common.Extensions;
    using Microsoft.SemanticKernel;

    public class ChatService(IMcpClientProvider clientProvider, ILogger<ChatService> logger /*, IKernel semanticKernel */) : IChatService
    {
        public List<ChatMessageViewModel> Messages { get; } =
        [
            new ChatMessageViewModel
            {
                Content = "Hello! I'm your AI assistant. How can I help you today?",
                Role = ChatRole.Assistant,
                Timestamp = DateTime.UtcNow
            }
        ];

        public async Task<string[]> GetToolsAsync()
        {
            var client = await clientProvider.GetClientAsync();
            var tools = await client.ListToolsAsync();

            return tools.Select(d => d.Name).ToArray();
        }

        public async Task<JsonNode> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters)
        {
            var client = await clientProvider.GetClientAsync();
            var result = await client.CallToolAsync(toolName, parameters);

            var textResult = (string)((dynamic)result.Content.FirstOrDefault())?.Text ?? string.Empty;
            var textResultIsValidJson = textResult.IsValidJson();
            var text = result.IsError == true ? JsonSerializer.Serialize(new { error = textResult }) 
                : (textResultIsValidJson ? textResult : JsonSerializer.Serialize(new { message = textResult }));

            return JsonNode.Parse(text);
        }

        public async Task<ChatMessageViewModel> SendMessageAsync(string message)
        {
            var userMessage = new ChatMessageViewModel {
                Content = message,
                Role = ChatRole.User,
                Timestamp = DateTime.UtcNow
            };
            Messages.Add(userMessage);

            var assistantMessage = new ChatMessageViewModel {
                Content = "...",
                Role = ChatRole.Assistant,
                IsProcessing = true,
                Timestamp = DateTime.UtcNow
            };
            Messages.Add(assistantMessage);

            try {
                logger.LogInformation($"Processing message: {message}");

                // TODO: Call the SemanticKernel to generate a response

                var result = GenerateAIResponse(message);
                assistantMessage.Content = result;

                logger.LogInformation($"Successfully generated response: {assistantMessage.Content[..Math.Min(assistantMessage.Content.Length, 50)]}...");

                assistantMessage.IsProcessing = false;
                return assistantMessage;
            } catch (Exception ex) {
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

        private string GenerateAIResponse(string message) 
        {
            message = message.ToLowerInvariant();

            if (message.Contains("hello") || message.Contains("hi") || message.Contains("hey")) {
                return "Hello! I'm your AI assistant. How can I help you today? I can answer questions, provide information, or just chat about various topics.";
            }

            if (message.Contains("what can you do") || message.Contains("capabilities") || message.Contains("help me")) {
                return "I'm an AI assistant designed to help with a variety of tasks. I can:\n\n" +
                       "- Answer questions on many topics\n" +
                       "- Explain complex concepts\n" +
                       "- Provide information on a wide range of subjects\n" +
                       "- Help with simple problem-solving\n\n" +
                       "What would you like assistance with today?";
            }

            if (message.Contains("weather") || message.Contains("temperature") || message.Contains("forecast")) {
                return "I don't have access to real-time weather data or forecasts. For accurate weather information, I'd recommend checking a weather service like AccuWeather, The Weather Channel, or your local meteorological service. Is there something else I can help with?";
            }

            if (message.Contains("ai") || message.Contains("artificial intelligence") || message.Contains("machine learning")) {
                return "Artificial Intelligence (AI) refers to systems designed to perform tasks that typically require human intelligence. Machine Learning is a subset of AI that focuses on algorithms that can learn from data. AI has various applications across industries including healthcare, finance, transportation, and more. It's an exciting and rapidly evolving field with both tremendous potential and important ethical considerations.";
            }

            if (message.Contains("programming") || message.Contains("code") || message.Contains("software")) {
                return "Software development is a fascinating field! There are many programming languages like C#, JavaScript, Python, and Java, each with their own strengths. Good software follows principles like SOLID, DRY, and emphasizes clean code practices. If you're looking to learn programming, I'd recommend starting with a language like Python for its readability, or JavaScript if you're interested in web development. Would you like more specific information about a particular aspect of software development?";
            }

            if (message.Contains("chat") || message.Contains("application") || message.Contains("app")) {
                return "This chat application is built using ASP.NET Core with C#. It features a responsive UI built with Bootstrap and uses modern JavaScript for the front-end interactions. The messages are rendered in real-time and support various formatting options. In a production environment, this would typically connect to an external AI service API like OpenAI's GPT or Azure OpenAI Service.";
            }

            if (message.Contains("bye") || message.Contains("goodbye") || message.Contains("see you")) {
                return "Goodbye! It was nice chatting with you. If you have more questions later, feel free to return and ask. Have a great day!";
            }

            if (message.Contains("thank") || message.Contains("thanks") || message.Contains("appreciate")) {
                return "You're welcome! I'm glad I could help. If you have any other questions or need assistance with anything else, don't hesitate to ask.";
            }

            return $"Thanks for your message: \"{message}\". While I'm just a simulated AI in this demo, a real LLM would provide a thoughtful response to your query. In a production environment, this would connect to a service like OpenAI GPT or Azure OpenAI to generate dynamic responses. How else can I assist you with this demonstration?";
        }
    }
}
