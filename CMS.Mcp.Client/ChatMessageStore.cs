namespace CMS.Mcp.Client
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Security.Contracts.Providers;

    public class ChatMessageStore(IServiceProvider serviceProvider) : IChatMessageStore
    {
        private readonly Dictionary<Guid, List<ChatMessageViewModel>> _messages = [];

        public Guid GetUserId()
        {
            var claimStoreProvider = serviceProvider.GetRequiredService<IClaimStoreProvider>();
            return claimStoreProvider.UserId.GetValueOrDefault();
        }

        public void Add(ChatMessageViewModel message)
        {
            var userId = GetUserId();
            if (!_messages.TryGetValue(userId, out var chatMessages))
            {
                chatMessages = [];
                _messages.Add(userId, chatMessages);
            }

            chatMessages.Add(message);
        }

        public ChatMessageViewModel[] List()
        {
            var userId = GetUserId();
            return !_messages.TryGetValue(userId, out var chatMessages) ? [] : chatMessages.ToArray();
        }

        public void Clear()
        {
            var userId = GetUserId();
            if (!_messages.TryGetValue(userId, out var chatMessages))
            {
                return;
            }

            chatMessages.Clear();
        }
    }
}