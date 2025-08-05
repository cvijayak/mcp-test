namespace CMS.Mcp.Client.Services
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Client;
    using Contracts;
    using Contracts.Models;
    using Contracts.Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.Connectors.OpenAI;

    public class SummaryService(IChatMessageStore chatMessageStore, ISummaryStore summaryStore, ILogger<SummaryService> logger) : ISummaryService
    {
        private const string SUMMARY_TEMPLATE = """
                                               Please generate a concise and well-organized summary of the conversation so far.

                                               Your summary will be used to guide future responses, so focus only on essential and relevant information. Remove any redundant, outdated, or irrelevant content. The goal is to preserve context that will help understand and respond to the user's upcoming messages accurately.

                                               Provide your response in the exact format below for easy parsing:

                                               <SUMMARY>
                                               ## CONVERSATION CONTEXT
                                               - Primary topic: [Briefly describe the main focus of the conversation]
                                               - User intent: [Summarize what the user is trying to achieve or solve]
                                               - Discussion progress: [Summarize how far the conversation has progressed toward a resolution]

                                               ## CRITICAL INFORMATION
                                               - User requirements: [List any specific needs, constraints, or goals shared by the user]
                                               - Key decisions: [Mention important choices or conclusions made so far]
                                               - Pending questions: [Highlight unresolved questions or requests]
                                               - System suggestions: [Summarize key recommendations provided by the assistant]

                                               ## TECHNICAL CONTEXT
                                               - System concepts: [Mention any important system-related ideas or frameworks discussed]
                                               - Tools mentioned: [List MCP tools, APIs, or components referenced]
                                               - Implementation details: [Summarize relevant technical code, architecture, or specs]
                                               - Error scenarios: [Note any errors, failures, or technical issues raised]

                                               ## CONTINUITY MARKERS
                                               - References to consider: [Identify conversation history that future responses should acknowledge]
                                               - Pending actions: [Mention next steps or actions that were agreed upon but not yet completed]
                                               - User preferences: [Note any preferences for style, tone, format, or approach stated by the user]
                                               </SUMMARY>

                                               Do not skip any section. If a section has no relevant content, write: "None."
                                               Keep the summary clean, relevant, and focused only on what is useful for continuing the conversation.
                                               """;

        public async Task SummarizeAsync(Kernel kernel)
        {
            var messages = (await chatMessageStore.ListAsync())
                .Where(m => !m.IsProcessing)
                .ToArray();
            if (messages.Length == 0)
            {
                return;
            }
            
            var conversationHistory = new StringBuilder();
            foreach (var message in messages)
            {
                string role = message.Role == ChatRole.User ? "User" : "Assistant";
                conversationHistory.AppendLine($"## {role}:");
                conversationHistory.AppendLine(message.Content);
                conversationHistory.AppendLine();
            }

            var existingSummary = await summaryStore.GetAsync();
            var summaryPrompt = $"""
                                    You are an expert at generating structured, context-aware summaries for LLM-driven conversations.

                                    Your summary will be stored and reused to maintain continuity in future interactions. Its goal is to preserve all important context without redundancy.

                                    Please summarize the following conversation using the provided structure. Focus on:
                                    - Key technical details, user goals, and decisions made so far
                                    - Open issues or pending follow-ups
                                    - User's preferences, requirements, or constraints
                                    - Any information that would be important for generating accurate, context-aware future responses

                                    Do NOT include irrelevant or repetitive information. Keep the summary clean and focused on what matters most for continuing the conversation.

                                    === FULL CONVERSATION HISTORY ===
                                    {conversationHistory}

                                    {SUMMARY_TEMPLATE}
                                    """;

            var contextualizedPrompt = !string.IsNullOrEmpty(existingSummary)
                ? $"""
                     {summaryPrompt}
                   
                     The following is the previously stored summary. Update and expand it based on the new conversation while preserving continuity:
                   
                     <SUMMARY>
                     {existingSummary}
                     </SUMMARY>
                   """
                : summaryPrompt;

            try
            {
                var promptExecutionSettings = new OpenAIPromptExecutionSettings 
                { 
                    Temperature = 0.1, 
                    MaxTokens = 500
                };
                var kernelArguments = new KernelArguments(promptExecutionSettings);
                var summaryResult = await kernel.InvokePromptAsync(contextualizedPrompt, kernelArguments);
                var rawSummary = summaryResult.GetValue<string>();
                var processedSummary = PromptingResponseUtility.ExtractSummary(rawSummary);
                
                await summaryStore.AddAsync(processedSummary);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating conversation summary");
            }
        }

        public async Task ClearSummaryAsync()
        {
            await summaryStore.ClearAsync();
        }

        public async Task<string> GetSummaryAsync()
        {
            return await summaryStore.GetAsync();
        }
    }
}
