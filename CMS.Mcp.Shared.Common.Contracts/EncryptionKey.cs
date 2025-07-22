namespace CMS.Mcp.Shared.Common.Contracts
{
    using Newtonsoft.Json;

    public class EncryptionKey : IJsonResource
	{
		public void Deconstruct(out string key, out string iv)
		{
			key = Key;
			iv = IV;
		}

		[JsonProperty("key")]
		public string Key { get; init; }

		[JsonProperty("iv")]
		public string IV { get; init; }
	}
}