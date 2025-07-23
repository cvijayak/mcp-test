namespace CMS.Mcp.Client
{
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Models;

    public class ChatMessageStore : IChatMessageStore
    {
        private readonly List<ChatMessageViewModel> _messages = [];

        public void Add(ChatMessageViewModel message) => _messages.Add(message);
        public ChatMessageViewModel[] List() => _messages.ToArray();
        public void Clear() => _messages.Clear();
    }
}