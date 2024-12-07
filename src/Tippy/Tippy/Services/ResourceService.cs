using System.IO;

using Dalamud.Plugin;

namespace Tippy.Services;

public class ResourceService
{
    private readonly string resourceDir;

    public ResourceService(IDalamudPluginInterface pluginInterface)
    {
        this.resourceDir = Path.Combine(pluginInterface.AssemblyLocation.DirectoryName!, "Resource");
    }

    /// <summary>
    /// Get resource path.
    /// </summary>
    /// <param name="fileName">resource to retrieve.</param>
    /// <returns>resource path.</returns>
    public string GetResourcePath(string fileName)
    {
        return Path.Combine(this.resourceDir, fileName);
    }
}
