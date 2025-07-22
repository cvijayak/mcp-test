namespace CMS.Mcp.Client.Contracts.Models
{
    using System;

    public class ChatMessageViewModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Content { get; set; } = string.Empty;
        public ChatRole Role { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsProcessing { get; set; }

        public string MediaUrl { get; set; }
        public string MediaType { get; set; }
    }
}
