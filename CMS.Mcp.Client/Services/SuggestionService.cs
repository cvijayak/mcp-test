namespace CMS.Mcp.Client.Services
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Client;
    using Contracts.Models;
    using Contracts.Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.Connectors.OpenAI;

    public class SuggestionService(Func<string, IMcpToolService> mcpToolServiceFactory, Func<string, Kernel> kernelFactory, ILogger<SuggestionService> logger) : ISuggestionService
    {
        private static readonly string[] INVALID_PHRASES =
        [
            "What else can I help",
            "Can you explain more",
            "Thanks for the information",
            "Tell me more",
            "How does that work",
            "Can you elaborate",
            "Interesting",
            "Please continue",
            "I understand",
            "Got it"
        ];

        private const string SUGGESTIONS_TEMPLATE = """
                                                    Please generate a list of concise and relevant suggestions based on the conversation so far.

                                                    Format your response exactly as shown below for consistent parsing:

                                                    <SUGGESTIONS>
                                                    - [Suggestion 1]
                                                    - [Suggestion 2]
                                                    - [Suggestion 3]
                                                    ...
                                                    </SUGGESTIONS>

                                                    Guidelines:
                                                    - Begin each suggestion with a dash (`-`) followed by a space.
                                                    - Include only clear, actionable, and technically useful suggestions.
                                                    - Avoid redundancy and irrelevant recommendations.
                                                    - Output only the content within the <SUGGESTIONS> tags.
                                                    """;

        private static string GetSuggestionPrompt(string serverName, string context) => $"""
                                                                                         You are a technical assistant specialized in MCP (Model Context Protocol) server tooling.

                                                                                         Your task is to generate and rank concise, actionable next-step suggestions for interacting with tools available on this MCP server.

                                                                                         === CURRENT SERVER CONTEXT ===
                                                                                         {context}

                                                                                         Instructions:
                                                                                         - Generate 10–12 suggestions that reference specific tools (by exact name).
                                                                                         - Each suggestion must be under 15 words, relevant, and technically actionable.

                                                                                         Ranking:
                                                                                         +50: Mentions exact tool name
                                                                                         +15: Uses verbs like "use", "run", "execute" near tool name
                                                                                         +10: Mentions server "{serverName}"
                                                                                         +5: Generic terms like "tool", "server", etc.
                                                                                         -30: Doesn't mention any listed tool

                                                                                         Constraints:
                                                                                         - Do not mention tools that are not listed above.
                                                                                         - Return only the top 5 ranked suggestions.

                                                                                         {SUGGESTIONS_TEMPLATE}
                                                                                         """;

        public async Task<string[]> ListAsync(string serverName, CancellationToken cancellationToken)
        {
            string[] Generate(McpToolViewModel[] tools) => tools.Take(5)
                .Select(t => $"How do I use the {t.Name} tool?")
                .ToArray();

            string GetMcpServerContext(McpToolViewModel[] tools)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"MCP Server: {serverName}");
                sb.AppendLine("Available Tools:");

                if (tools.Length > 0)
                {
                    foreach (var tool in tools)
                    {
                        sb.AppendLine($"- {tool.Name}: {tool.Description}");
                    }
                }
                else
                {
                    sb.AppendLine("- No tools information available for this server");
                }

                return sb.ToString();
            }

        var mcpToolService = mcpToolServiceFactory(serverName);
            var serverTools = await mcpToolService.ListAsync(cancellationToken);
            var context = GetMcpServerContext(serverTools);
            var prompt = GetSuggestionPrompt(serverName, context);

            var kernel = kernelFactory(serverName);
            if (kernel == null)
            {
                return [];
            }

            try
            {
                var executionSettings = new OpenAIPromptExecutionSettings { Temperature = 0.4, MaxTokens = 200 };
                var arguments = new KernelArguments(executionSettings);
                var response = await kernel.InvokePromptAsync(prompt, arguments, cancellationToken: cancellationToken);

                var raw = response.GetValue<string>();
                logger.LogDebug("Raw suggestions response: {Response}", raw);

                var suggestions = ParseSuggestionResponse(raw);
                if (suggestions.Length > 0)
                {
                    return suggestions;
                }

                return Generate(serverTools);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating suggestions");
                return [];
            }
        }

        public static string[] ParseSuggestionResponse(string raw)
        {
            string[] ParseSuggestions(string response)
            {
                var suggestionsContent = PromptingResponseUtility.ExtractSuggestions(response);
                return suggestionsContent
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim())
                    .Select(line => line.StartsWith("-") ? line.Substring(1).Trim() : line)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToArray();
            }

            string Clean(string line)
            {
                line = Regex.Replace(line.Trim(), @"^\s*(?:[-•*]|\d+[\.\)]|'|"")\s*", "");
                line = Regex.Replace(line, @"^(User:|Suggestion:|Ask:|Question:|You could say:|Follow-up:)\s*", "", RegexOptions.IgnoreCase);

                if (!string.IsNullOrEmpty(line) && char.IsLower(line[0]))
                {
                    line = char.ToUpper(line[0]) + line[1..];
                }

                if (!string.IsNullOrEmpty(line) && !".?!".Contains(line[^1]))
                {
                    line += line.Contains('?') ? "?" : ".";
                }

                return line;
            }

            bool IsValid(string line) =>
                !string.IsNullOrWhiteSpace(line) && line.Length is >= 5 and <= 120 && INVALID_PHRASES.All(p => !line.StartsWith(p, StringComparison.OrdinalIgnoreCase));

            return ParseSuggestions(raw)
                .Select(Clean)
                .Where(IsValid)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .ToArray();
        }
    }
}
