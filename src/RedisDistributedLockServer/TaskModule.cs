using Autofac;
using ConductorSharp.Engine.Extensions;
using RedisDistributedLockServer.Handlers;
using RedisDistributedLockServer.Workflows;

namespace RedisDistributedLockServer
{
    public class TaskModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterWorkerTask<WaitTask.Handler>();
            builder.RegisterWorkerTask<LockResource.Handler>();
            builder.RegisterWorkerTask<ReleaseLock.Handler>();
            builder.RegisterWorkflow<TestWf>();
        }
    }
}
