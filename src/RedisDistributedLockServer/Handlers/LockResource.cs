using ConductorSharp.Engine;
using ConductorSharp.Engine.Util;
using MediatR;
using Microsoft.Extensions.Options;
using RedisDistributedLockServer.Abstractions;
using RedisDistributedLockServer.Options;
using RedLockNet;

namespace RedisDistributedLockServer.Handlers
{
    public record LockResource : IRequest<LockResource.Response>
    {
        public required string Resource { get; init; }
        public required string WorkflowId { get; init; }
        public int ExpiryTime { get; init; }
        public int TotalAcquireLockTime { get; init; }
        public int RetryAcquireLockTime { get; set; }

        public record Response
        {
            public required string LockId { get; init; }
            public required string Status { get; set; }
            public required string Resource { get; set; }
        }

        [OriginalName("LOCK_acquire")]
        public class Handler : TaskRequestHandler<LockResource, Response>
        {
            private readonly IDistributedLockFactory _lockFactory;
            private readonly IRedLockBufferService _lockBufferService;
            private readonly RedisOptions _redisOptions;

            public Handler(
                IDistributedLockFactory lockFactory,
                IOptions<RedisOptions> redisOptions,
                IRedLockBufferService lockBufferService
            ) =>
                (_lockFactory, _lockBufferService, _redisOptions) = (
                    lockFactory,
                    lockBufferService,
                    redisOptions.Value
                );

            public override async Task<Response> Handle(
                LockResource request,
                CancellationToken cancellationToken
            )
            {
                var expiryTime =
                    request.ExpiryTime <= 0
                        ? _redisOptions.DefaultLockAutoreleaseTime
                        : request.ExpiryTime;

                var waitTime =
                    request.TotalAcquireLockTime <= 0
                        ? _redisOptions.DefaultAcquireLockTime
                        : request.TotalAcquireLockTime;

                var retryTime =
                    request.RetryAcquireLockTime <= 0
                        ? _redisOptions.DefaultRetryAcquireLockTime
                        : request.RetryAcquireLockTime;

                var acquiredLock = await _lockFactory.CreateLockAsync(
                    request.Resource,
                    request.WorkflowId,
                    TimeSpan.FromMinutes(expiryTime),
                    TimeSpan.FromMinutes(waitTime),
                    TimeSpan.FromSeconds(retryTime),
                    cancellationToken
                );

                if (acquiredLock.IsAcquired)
                {
                    _lockBufferService.Add(acquiredLock.LockId, acquiredLock);

                    return new Response
                    {
                        LockId = acquiredLock.LockId,
                        Status = acquiredLock.Status.ToString(),
                        Resource = acquiredLock.Resource
                    };
                }
                else
                    throw new InvalidOperationException(
                        $"Couldn't acquire the lock for resource: {request.Resource}. Status: {acquiredLock.Status}"
                    );
            }
        }
    }
}
