namespace CMS.Mcp.Shared.Configuration
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class ConfigurationReaderBase
    {
        private const string LOGGING_SECTION_NAME = "Logging";

        private readonly IServiceCollection _services;
        private readonly IConfiguration _configuration;

        protected ConfigurationReaderBase(IServiceCollection services, IConfiguration configuration)
        {
            _services = services;
            _configuration = configuration;
        }

        protected TOptions GetOptions<TOptions>(string sectionName) where TOptions : class => _configuration.GetSection(sectionName).Get<TOptions>();
        protected void Bind<TOptions>(string sectionName) where TOptions : class => _services.AddOptions<TOptions>().Bind(_configuration.GetSection(sectionName));
        protected void Bind<TOptions>(string sectionName, TOptions options) where TOptions : class => _configuration.GetSection(sectionName).Bind(options);
        protected string GetConnectionString(string sectionName) => _configuration.GetConnectionString(sectionName);

        protected IConfiguration GetSection(string sectionName) => _configuration.GetSection(sectionName);

        public IConfiguration LoggingSection => GetSection(LOGGING_SECTION_NAME);
    }
}