using RedLockNet;
using RedLockNet.SERedis;
using System.Collections.Concurrent;

namespace RedisDistributedLockServer.Abstractions
{
    public interface IRedLockBufferService
    {
        ConcurrentDictionary<string, IRedLock> Buffer { get; }

        Task UnlockAsync(string lockId);
        void Add(string lockId, IRedLock redLock);
        Task UnlockRangeAsync(params string[] lockIds);
        IRedLock? GetRedLock(string lockId);
    }
}
