using System.Reflection;

namespace RedisDistributedLockServer.EmbeddedResource
{
    internal static class EmbeddedResourceLoader
    {
        internal static string GetEmbeddedResource(string resource, Assembly assembly)
        {
            var resourceName = $"{assembly.GetName().Name}.{resource}";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var streamReader = new StreamReader(stream!);
            return streamReader.ReadToEnd();
        }
    }
}
