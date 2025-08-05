namespace CMS.Mcp.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Security.Contracts.Providers;

    public class ChatMessageStore(IServiceProvider serviceProvider) : IChatMessageStore
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly Dictionary<Guid, List<ChatMessageViewModel>> _messages = [];

        private async Task<T> ExecuteUnderSemaphore<T>(Func<T> action)
        {
            await _semaphore.WaitAsync();
            try
            {
                return action();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task ExecuteUnderSemaphore(Action action)
        {
            await _semaphore.WaitAsync();
            try
            {
                action();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Guid GetUserId()
        {
            var claimStoreProvider = serviceProvider.GetRequiredService<IClaimStoreProvider>();
            return claimStoreProvider.UserId.GetValueOrDefault();
        }

        public async Task AddAsync(ChatMessageViewModel message)
        {
            await ExecuteUnderSemaphore(() =>
            {
                var userId = GetUserId();
                if (!_messages.TryGetValue(userId, out var chatMessages))
                {
                    chatMessages = [];
                    _messages.Add(userId, chatMessages);
                }

                chatMessages.Add(message);
            });
        }

        public async Task<ChatMessageViewModel[]> ListAsync()
        {
            return await ExecuteUnderSemaphore(() =>
            {
                var userId = GetUserId();
                return !_messages.TryGetValue(userId, out var chatMessages) ? [] : chatMessages.ToArray();
            });
        }

        public async Task ClearAsync()
        {
            await ExecuteUnderSemaphore(() =>
            {
                var userId = GetUserId();
                if (!_messages.TryGetValue(userId, out var chatMessages))
                {
                    return;
                }

                chatMessages.Clear();
            });
        }
    }
}