using ConductorSharp.Engine;
using ConductorSharp.Engine.Util;
using MediatR;
using RedisDistributedLockServer.Abstractions;
using StackExchange.Redis;

namespace RedisDistributedLockServer.Handlers
{
    public record ReleaseLock : IRequest<ReleaseLock.Response>
    {
        public required string Resource { get; init; }
        public required string WorkflowId { get; init; }

        public record Response
        {
            public required string WorkfowId { get; init; }
        }

        [OriginalName("LOCK_release")]
        public class Handler : TaskRequestHandler<ReleaseLock, Response>
        {
            private readonly IRedLockBufferService _redLockBuffer;
            private readonly IDatabase _redisDb;

            public Handler(IRedLockBufferService redLockBuffer, IDatabase redisDb)
            {
                _redLockBuffer = redLockBuffer;
                _redisDb = redisDb;
            }

            public override async Task<Response> Handle(
                ReleaseLock request,
                CancellationToken cancellationToken
            )
            {
                if (
                    _redLockBuffer.Buffer.TryGetValue(request.WorkflowId, out var @lock)
                    && @lock.LockId == request.WorkflowId
                    && @lock.Resource == request.Resource
                )
                {
                    await @lock.DisposeAsync();
                    var resource = (string)(await _redisDb.StringGetAsync(request.Resource));

                    if (resource is not null && resource != request.WorkflowId)
                        throw new InvalidOperationException(
                            $"Resource: {request.Resource} is locked by different workflow: {resource}."
                        );
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Couldn't release specified resource: {request.Resource} for workflwo with id: {request.WorkflowId} or resource is already released."
                    );
                }

                return new Response { WorkfowId = request.WorkflowId };
            }
        }
    }
}
