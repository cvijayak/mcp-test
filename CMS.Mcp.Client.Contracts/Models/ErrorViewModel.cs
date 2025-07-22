namespace CMS.Mcp.Client.Contracts.Models
{
    public class ErrorViewModel
    {
        public int StatusCode { get; init; }
        public string Message { get; init; }
        public string RequestId { get; init; }
        public bool ShowRequestId { get; init; }
    }
}
