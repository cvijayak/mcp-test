namespace CMS.Mcp.Shared.Api.Clients.Extensions
{
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Contracts.Exceptions;

    internal static class HttpResponseMessageX 
    {
        public static async Task<ExternalApiException> BuildAsync(this HttpResponseMessage response, string serverType)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var failure = string.IsNullOrEmpty(responseContent) ? null : responseContent;
            var responseHeaders = response.Headers.Select(hdr => new
            {
                hdr.Key,
                Value = string.Join(",", hdr.Value)
            }).GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.First().Value);

            return new ExternalApiException(serverType, response.StatusCode, responseHeaders, failure);
        }
    }
}