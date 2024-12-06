using Tippy.Mediator;

using DalaMock.Core.Mocks;
using DalaMock.Host.Mediator;

using Microsoft.Extensions.Hosting;

namespace TippyMock;

public class MockBootService(
    MediatorService mediatorService,
    MockWindow mockWindow) : IHostedService, IMediatorSubscriber
{
    public MediatorService MediatorService { get; set; } = mediatorService;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.MediatorService.Subscribe<PluginLoadedMessage>(this, this.PluginLoaded);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.MediatorService.UnsubscribeAll(this);
        return Task.CompletedTask;
    }

    private void PluginLoaded(PluginLoadedMessage obj)
    {
        mockWindow.IsOpen = true;
    }
}
