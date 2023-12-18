using ConductorSharp.Client.Service;
using RedisDistributedLockServer.Abstractions;
using RedisDistributedLockServer.EmbeddedResource;
using StackExchange.Redis;

namespace RedisDistributedLockServer.BackgroundServices
{
    public class LockAutoreleaseService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        private static readonly string _getKeysScript = EmbeddedResourceLoader.GetEmbeddedResource(
            "EmbeddedResource.Lua.GetRedisKeys.lua",
            typeof(LockAutoreleaseService).Assembly
        );

        private static readonly string _deleteKeys = EmbeddedResourceLoader.GetEmbeddedResource(
            "EmbeddedResource.Lua.DeleteKeys.lua",
            typeof(LockAutoreleaseService).Assembly
        );

        private static readonly string _getAllKeysAndValues =
            EmbeddedResourceLoader.GetEmbeddedResource(
                "EmbeddedResource.Lua.GetAllKeysValues.lua",
                typeof(LockAutoreleaseService).Assembly
            );

        public LockAutoreleaseService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await DoWork();

                await Task.Delay(3000, stoppingToken);
            }
        }

        private async Task DoWork()
        {
            using var scope = _serviceProvider.CreateScope();
            //var redisDb = scope.ServiceProvider.GetRequiredService<IDatabase>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger<LockAutoreleaseService>();

            var bufferService = scope.ServiceProvider.GetRequiredService<IRedLockBufferService>();

            //var keys = await redisDb.ScriptEvaluateAsync(_getAllKeysAndValues);

            //var data = ((RedisResult[])keys)
            //    .Select(x => new { Key = ((RedisResult[])x)[0], Value = ((RedisResult[])x)[1] })
            //    .ToDictionary(x => (string)x.Key, x => (string)x.Value);

            List<Task<(string, string, bool)>> values = new();

            foreach (var key in bufferService.Buffer.Keys)
            {
                var redLock = bufferService.Buffer[key];

                var task = CheckWorkflowStatus(key, redLock.LockId);

                logger.LogInformation(
                    $"Locked resource: {redLock.Resource} by workflow: {redLock.LockId}."
                );

                values.Add(task);
            }

            var resourcesToUnlock = (await Task.WhenAll(values))
                .Where(x => x.Item3)
                .Select(x => new { Resource = x.Item1, LockId = x.Item2 })
                .ToArray();

            if (resourcesToUnlock.Any())
            {
                var lockIds = resourcesToUnlock.Select(x => x.LockId).ToArray();

                await bufferService.UnlockRangeAsync(lockIds);
            }
        }

        private async Task<(string, string, bool)> CheckWorkflowStatus(
            string resource,
            string workflowId
        )
        {
            using var scope = _serviceProvider.CreateScope();
            var workflowService = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

            var workflowStatus = await workflowService.GetWorkflowStatus(
                workflowId,
                includeTasks: false
            );

            return (resource, workflowId, workflowStatus.Status != "RUNNING");
        }

        //public override async Task StopAsync(CancellationToken cancellationToken)
        //{
        //    await ReleaseLocks();
        //}

        private async Task ReleaseLocks(string[] keys)
        {
            using var scope = _serviceProvider.CreateScope();
            var redisDb = scope.ServiceProvider.GetRequiredService<IDatabase>();

            await redisDb.ScriptEvaluateAsync(
                _deleteKeys,
                null,
                keys.Select(x => (RedisValue)x).ToArray()
            );
        }
    }
}
