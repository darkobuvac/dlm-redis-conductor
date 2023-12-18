using ConductorSharp.Engine;
using ConductorSharp.Engine.Model;
using MediatR;

namespace RedisDistributedLockServer.Handlers
{
    public record WaitTask : IRequest<NoOutput>
    {
        public int Seconds { get; set; }

        public class Handler : TaskRequestHandler<WaitTask, NoOutput>
        {
            public override async Task<NoOutput> Handle(
                WaitTask request,
                CancellationToken cancellationToken
            )
            {
                await Task.Delay(request.Seconds * 1000, cancellationToken);

                return new NoOutput();
            }
        }
    }
}
