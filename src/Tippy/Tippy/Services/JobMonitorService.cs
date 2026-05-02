using System;
using System.Threading;
using System.Threading.Tasks;

using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;

namespace Tippy.Services;

public class JobMonitorService : IHostedService
{
    private readonly IObjectTable objectTable;
    private readonly IFramework framework;
    private uint currentJobId;
    private uint currentRoleId;

    public JobMonitorService(IObjectTable objectTable, IFramework framework)
    {
        this.objectTable = objectTable;
        this.framework = framework;
    }

    public delegate void JobStateChangedDelegate(uint jobId, uint roleId);

    public event JobStateChangedDelegate? JobStateChanged;

    public uint CurrentJobId => this.currentJobId;

    public uint CurrentRoleId => this.currentRoleId;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.framework.Update += this.FrameworkOnUpdate;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.framework.Update -= this.FrameworkOnUpdate;
        return Task.CompletedTask;
    }

    private void FrameworkOnUpdate(IFramework framework)
    {
        try
        {
            if (this.objectTable.LocalPlayer == null || !this.objectTable.LocalPlayer.ClassJob.IsValid)
            {
                return;
            }

            if (this.objectTable.LocalPlayer.ClassJob.RowId != this.CurrentJobId)
            {
                this.currentJobId = this.objectTable.LocalPlayer.ClassJob.RowId;
                this.currentRoleId = this.objectTable.LocalPlayer.ClassJob.Value.Role;
                this.JobStateChanged?.Invoke(this.CurrentJobId, this.CurrentRoleId);
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }
}
