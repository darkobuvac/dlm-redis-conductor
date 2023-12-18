using Autofac;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Health;

namespace RedisDistributedLockServer
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder AddWorkflows(
            this IHostBuilder hostBuilder,
            IConfiguration configuration
        )
        {
            return hostBuilder.ConfigureContainer<ContainerBuilder>(
                (context, builder) =>
                {
                    builder
                        .AddConductorSharp(
                            baseUrl: configuration.GetValue<string>("Conductor:BaseUrl"),
                            apiPath: configuration.GetValue<string>("Conductor:ApiUrl")
                        )
                        .AddExecutionManager(
                            maxConcurrentWorkers: 10,
                            sleepInterval: 50,
                            longPollInterval: 100,
                            domain: null,
                            typeof(HostBuilderExtensions).Assembly
                        )
                        .SetHealthCheckService<InMemoryHealthService>()
                        .AddPipelines(pipelines =>
                        {
                            pipelines.AddExecutionTaskTracking();
                            pipelines.AddContextLogging();
                            pipelines.AddRequestResponseLogging();
                            pipelines.AddValidation();
                        });

                    builder.RegisterModule<TaskModule>();
                }
            );
        }
    }
}
