namespace CMS.Mcp.Shared.Common.Extensions
{
    using System.Text.Json;

    public static class StringX
    {
        public static string ToCamelCase(this string name) => string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];

        public static bool IsValidJson(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}