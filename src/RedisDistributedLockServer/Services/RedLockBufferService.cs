using RedisDistributedLockServer.Abstractions;
using RedLockNet;
using RedLockNet.SERedis;
using System.Collections.Concurrent;

namespace RedisDistributedLockServer.Services
{
    public class RedLockBufferService : IRedLockBufferService
    {
        private readonly ConcurrentDictionary<string, IRedLock> _buffer = new();
        private readonly ILogger<RedLockBufferService> _logger;

        public ConcurrentDictionary<string, IRedLock> Buffer => _buffer;

        public RedLockBufferService(ILogger<RedLockBufferService> logger) => (_logger) = (logger);

        public void Add(string lockId, IRedLock redLock)
        {
            _buffer.TryAdd(lockId, redLock);
        }

        public async Task UnlockAsync(string lockId)
        {
            if (_buffer.TryGetValue(lockId, out var redLock))
            {
                _logger.LogInformation(
                    $"Releasing lock for resource: {redLock.Resource} with id: {redLock.LockId}."
                );
                await redLock.DisposeAsync();

                _buffer.TryRemove(lockId, out _);
            }
        }

        public async Task UnlockRangeAsync(params string[] lockIds)
        {
            foreach (var lockId in lockIds)
            {
                await UnlockAsync(lockId);
            }
        }

        public IRedLock? GetRedLock(string lockId)
        {
            if (_buffer.TryGetValue(lockId, out var redLock))
                return redLock;

            return null;
        }
    }
}
