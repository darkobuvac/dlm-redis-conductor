using Microsoft.Extensions.Options;
using RedisDistributedLockServer.Options;
using RedLockNet.SERedis.Configuration;
using RedLockNet.SERedis;
using RedLockNet;
using System.Net;
using StackExchange.Redis;
using RedisDistributedLockServer.Abstractions;
using RedisDistributedLockServer.Services;

namespace RedisDistributedLockServer
{
    public static class ServiceExtensions
    {
        public static IServiceCollection RegisterServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddSingleton<IDistributedLockFactory, RedLockFactory>(sp =>
            {
                var redisOptions =
                    sp.GetRequiredService<IOptions<RedisOptions>>().Value
                    ?? throw new Exception($"Redis options are not configured!");

                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                var lockFactory = RedLockFactory.Create(
                    new List<RedLockEndPoint>
                    {
                        new RedLockEndPoint
                        {
                            EndPoint = new DnsEndPoint(redisOptions.Hostname, redisOptions.Port),
                            Password = redisOptions.Password,
                            RedisKeyFormat = "{0}"
                        },
                    },
                    loggerFactory
                );

                return lockFactory;
            });

            services.AddScoped(sp =>
            {
                var redisOptions =
                    sp.GetRequiredService<IOptions<RedisOptions>>().Value
                    ?? throw new Exception($"Redis options are not configured!");

                var connectionString =
                    $"{redisOptions.Hostname}:{redisOptions.Port},password={redisOptions.Password}";

                IConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(
                    connectionString
                );

                return multiplexer.GetDatabase();
            });

            services.AddSingleton<IRedLockBufferService, RedLockBufferService>();

            return services;
        }
    }
}
