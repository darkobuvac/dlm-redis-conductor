namespace RedisDistributedLockServer.Options;

public class RedisOptions
{
    public static readonly string SectionName = "Redis";

    public required string Hostname { get; set; }

    public required int Port { get; set; }

    public required string Password { get; set; }

    public int DefaultLockAutoreleaseTime { get; set; }
    public int DefaultAcquireLockTime { get; set; }
    public int DefaultRetryAcquireLockTime { get; set; }
}
