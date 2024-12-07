using Autofac;
using DalaMock.Host.Hosting;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using Tippy.Services;

namespace Tippy;

/// <inheritdoc />
public class TippyPlugin : HostedPlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TippyPlugin"/> class.
    /// </summary>
    public TippyPlugin(
        IDalamudPluginInterface pluginInterface,
        IPluginLog pluginLog,
        ICommandManager commandManager,
        IClientState clientState,
        IFramework framework,
        ITextureProvider textureProvider)
        : base(pluginInterface, pluginLog, commandManager, clientState, framework, textureProvider)
    {
        this.CreateHost();
        this.Start();
    }

    public string Name => "Tippy";

    public override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        // Windows
        containerBuilder.RegisterType<TippyUI>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<ConfigWindow>().As<Window>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<CommandService>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<ConfigurationLoaderService>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<JobMonitorService>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<ResourceService>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<TippyAPI>().AsImplementedInterfaces().AsSelf().SingleInstance();
        containerBuilder.RegisterType<TippyController>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<TippyProvider>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<TextHelperService>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<TranslationService>().AsSelf().SingleInstance();

        containerBuilder.Register(
            s =>
            {
                var configurationLoaderService = s.Resolve<ConfigurationLoaderService>();
                return configurationLoaderService.GetConfiguration();
            }).SingleInstance();
    }

    public override void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHostedService(p => p.GetRequiredService<ConfigurationLoaderService>());
        serviceCollection.AddHostedService(p => p.GetRequiredService<TranslationService>());
        serviceCollection.AddHostedService(p => p.GetRequiredService<JobMonitorService>());
        serviceCollection.AddHostedService(p => p.GetRequiredService<TippyController>());
        serviceCollection.AddHostedService(p => p.GetRequiredService<TippyUI>());
        serviceCollection.AddHostedService(p => p.GetRequiredService<TippyProvider>());
    }
}
