namespace RedisDistributedLockServer.Util
{
    public static class RedisKey
    {
        public static string CreateKey(string resource, string value) => $"{resource}|{value}";

        public static (string, string) SeparateKey(string key)
        {
            var parts = key.Split('|');
            if (parts.Length == 2)
            {
                return (parts[0], parts[1]);
            }
            else
            {
                throw new ArgumentException($"Invalid key format: {key}");
            }
        }
    }
}
