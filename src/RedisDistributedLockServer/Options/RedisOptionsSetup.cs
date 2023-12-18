using Microsoft.Extensions.Options;

namespace RedisDistributedLockServer.Options
{
    public class RedisOptionsSetup : IConfigureOptions<RedisOptions>
    {
        private readonly IConfiguration _configuration;

        public RedisOptionsSetup(IConfiguration configuration) => _configuration = configuration;

        public void Configure(RedisOptions options) =>
            _configuration.GetSection(RedisOptions.SectionName).Bind(options);
    }
}
