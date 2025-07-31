namespace CMS.Mcp.Client
{
    using System.Text.RegularExpressions;

    public static class PromptingResponseUtility
    {
        private static string ExtractTaggedContent(string response, string tagName)
        {
            if (string.IsNullOrEmpty(response))
            {
                return string.Empty;
            }

            var pattern = $@"<{tagName}>(.*?)</{tagName}>";
            var match = Regex.Match(response, pattern, RegexOptions.Singleline);

            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return response;
        }

        public static string ExtractSummary(string response) => ExtractTaggedContent(response, "SUMMARY");
        public static string ExtractSuggestions(string response) => ExtractTaggedContent(response, "SUGGESTIONS");
    }
}
