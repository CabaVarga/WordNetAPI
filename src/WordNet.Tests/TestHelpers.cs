using System.IO;

namespace WordNet.Tests;

internal static class TestHelpers
{
    internal static string FindResourcesDirectory()
    {
        DirectoryInfo? current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            string candidate = Path.Combine(current.FullName, "resources");
            if (Directory.Exists(candidate))
                return candidate;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository resources directory.");
    }
}
