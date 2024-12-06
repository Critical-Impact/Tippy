using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;

namespace Tippy.Services;

public class JobMonitorService : IHostedService
{
    private readonly IClientState clientState;
    private readonly IFramework framework;
    private uint currentJobId;
    private uint currentRoleId;

    public delegate void JobStateChangedDelegate(uint jobId, uint roleId);

    public event JobStateChangedDelegate JobStateChanged;

    public JobMonitorService(IClientState clientState, IFramework framework)
    {
        this.clientState = clientState;
        this.framework = framework;
    }

    public uint CurrentJobId => this.currentJobId;

    public uint CurrentRoleId => this.currentRoleId;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.framework.Update += this.FrameworkOnUpdate;
        return Task.CompletedTask;
    }

    private void FrameworkOnUpdate(IFramework framework)
    {
        try
        {
            if (this.clientState.LocalPlayer == null || !this.clientState.LocalPlayer.ClassJob.IsValid) return;
            if (this.clientState.LocalPlayer.ClassJob.RowId != this.CurrentJobId)
            {
                this.currentJobId = this.clientState.LocalPlayer.ClassJob.RowId;
                this.currentRoleId = this.clientState.LocalPlayer.ClassJob.Value.Role;
                this.JobStateChanged.Invoke(this.CurrentJobId, this.CurrentRoleId);
            }
        }
        catch (Exception)
        {
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.framework.Update -= this.FrameworkOnUpdate;
        return Task.CompletedTask;
    }
}
