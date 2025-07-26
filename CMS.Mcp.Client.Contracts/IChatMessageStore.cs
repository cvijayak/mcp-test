namespace CMS.Mcp.Client.Contracts
{
    using Models;

    public interface IChatMessageStore
    {
        void Add(ChatMessageViewModel message);
        ChatMessageViewModel[] List();
        void Clear();
    }
}