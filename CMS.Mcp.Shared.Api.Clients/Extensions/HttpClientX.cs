namespace CMS.Mcp.Shared.Api.Clients.Extensions
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class HttpClientX
    {
        public static async Task<HttpResponseMessage> SendAsync(this HttpClient httpClient, string serverType, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await httpClient.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException exception)
            {
                throw exception.BuildServiceUnavailable(serverType);
            }
            catch (TaskCanceledException exception)
            {
                throw exception.BuildGatewayTimeout(serverType);
            }
        }
    }
}