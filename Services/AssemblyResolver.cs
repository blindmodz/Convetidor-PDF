using System.Reflection;

internal static class AssemblyResolver
{
    public static void Register()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (_, eventArgs) =>
        {
            var requestedName = new AssemblyName(eventArgs.Name).Name + ".dll";
            var resourceName = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceNames()
                .FirstOrDefault(name => string.Equals(name, requestedName, StringComparison.OrdinalIgnoreCase));

            if (resourceName is null)
            {
                return null;
            }

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            using var buffer = new MemoryStream();
            stream!.CopyTo(buffer);
            return Assembly.Load(buffer.ToArray());
        };
    }
}
