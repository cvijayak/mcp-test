namespace CMS.Mcp.Shared.Configuration.Providers
{
    using Common;
    using Common.Contracts;
    using Microsoft.Extensions.Configuration.Json;

    internal class SecretConfigurationProvider : ConfigurationProvider
	{
		private readonly AesCrypto _aesCrypto;

		public SecretConfigurationProvider(JsonConfigurationSource source, IResourceReader resourceReader) : base(source)
		{
			var (key, iv) = resourceReader.ReadAsJson<EncryptionKey>();
			_aesCrypto = new AesCrypto(key, iv);
		}

		protected override string Convert(string configData) => _aesCrypto.Decrypt(configData);
	}
}