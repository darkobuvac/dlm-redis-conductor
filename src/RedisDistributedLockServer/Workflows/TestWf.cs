using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Util;
using RedisDistributedLockServer.Handlers;
using static RedisDistributedLockServer.Workflows.TestWf;

namespace RedisDistributedLockServer.Workflows
{
    [OriginalName("LOCK_test_wf")]
    public class TestWf : Workflow<TestWf, TestWfInput, TestWfOutput>
    {
        public TestWf(WorkflowDefinitionBuilder<TestWf, TestWfInput, TestWfOutput> builder)
            : base(builder) { }

        public class TestWfInput : WorkflowInput<TestWfOutput>
        {
            public required string Resource { get; set; }
        }

        public class TestWfOutput : WorkflowOutput { }

        public LockResource.Handler LockResource { get; set; }
        public WaitTask.Handler WaitTask { get; set; }

        public override void BuildDefinition()
        {
            base.BuildDefinition();

            _builder.AddTask(
                taskSelector => taskSelector.LockResource,
                wf =>
                    new LockResource
                    {
                        Resource = wf.Input.Resource,
                        WorkflowId = "${workflow.workflowId}",
                        ExpiryTime = 2,
                        TotalAcquireLockTime = 1
                    }
            );

            _builder.AddTask(
                taskSelector => taskSelector.WaitTask,
                wf => new WaitTask { Seconds = 30 }
            );
        }
    }
}
