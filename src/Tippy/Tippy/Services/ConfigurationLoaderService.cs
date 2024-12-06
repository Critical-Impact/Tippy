using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;

namespace Tippy.Services;

public class ConfigurationLoaderService(
    IDalamudPluginInterface pluginInterface,
    IPluginLog pluginLog) : IHostedService
{
    private TippyConfig? configuration;

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
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
        this.Save();
        pluginLog.Verbose("Stopping configuration loader, saving.");
        return Task.CompletedTask;
    }
}
