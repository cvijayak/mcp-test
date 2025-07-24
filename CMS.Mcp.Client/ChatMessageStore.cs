namespace CMS.Mcp.Client
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Models;
    using Security.Contracts.Providers;

    public class ChatMessageStore(ISessionProvider sessionProvider) : IChatMessageStore
    {
        private readonly Dictionary<string, List<ChatMessageViewModel>> _messages = [];

        public async Task AddAsync(ChatMessageViewModel message)
        {
            var accessToken = await sessionProvider.GetAccessTokenAsync();
            if (!_messages.TryGetValue(accessToken, out var chatMessages))
            {
                chatMessages = [];
                _messages.Add(accessToken, chatMessages);
            }

            chatMessages.Add(message);
        }

        public async Task<ChatMessageViewModel[]> ListAsync()
        {
            var accessToken = await sessionProvider.GetAccessTokenAsync();
            return !_messages.TryGetValue(accessToken, out var chatMessages) ? [] : chatMessages.ToArray();
        }

        public async Task ClearAsync()
        {
            var accessToken = await sessionProvider.GetAccessTokenAsync();
            if (!_messages.TryGetValue(accessToken, out var chatMessages))
            {
                return;
            }

            chatMessages.Clear();
        }
    }
}