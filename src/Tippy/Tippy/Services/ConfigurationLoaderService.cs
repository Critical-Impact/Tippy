using System;
using System.Threading;
using System.Threading.Tasks;

using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;

namespace Tippy.Services;

public class ConfigurationLoaderService(
    IDalamudPluginInterface pluginInterface,
    IPluginLog pluginLog,
    IFramework framework) : IHostedService
{
    private TippyConfig? configuration;

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        framework.Update += this.FrameworkOnUpdate;
        return Task.CompletedTask;
    }

    private void FrameworkOnUpdate(IFramework framework1)
    {
        if (this.configuration?.IsDirty ?? false)
        {
            this.Save();
        }
    }

    public TippyConfig GetConfiguration()
    {
        if (this.configuration == null)
        {
            try
            {
                this.configuration = pluginInterface.GetPluginConfig() as TippyConfig ??
                                     new TippyConfig();
            }
            catch (Exception e)
            {
                pluginLog.Error(e, "Failed to load configuration");
                this.configuration = new TippyConfig();
            }
        }

        return this.configuration;
    }

    public void Save()
    {
        this.GetConfiguration().IsDirty = false;
        pluginInterface.SavePluginConfig(this.GetConfiguration());
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        framework.Update -= this.FrameworkOnUpdate;
        this.Save();
        pluginLog.Verbose("Stopping configuration loader, saving.");
        return Task.CompletedTask;
    }
}
