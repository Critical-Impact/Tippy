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

    public override HostedPluginOptions ConfigureOptions()
    {
        return new HostedPluginOptions()
        {
            UseMediatorService = false,
        };
    }

    public override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        // Windows
        containerBuilder.RegisterType<ConfigWindow>().As<Window>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<CommandService>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<ResourceService>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<TippyAPI>().AsImplementedInterfaces().AsSelf().SingleInstance();
        containerBuilder.RegisterType<TextHelperService>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<FontService>().AsImplementedInterfaces().SingleInstance();
        this.RegisterHostedService(typeof(ConfigurationLoaderService));
        this.RegisterHostedService(typeof(TranslationService));
        this.RegisterHostedService(typeof(JobMonitorService));
        this.RegisterHostedService(typeof(TippyController));
        this.RegisterHostedService(typeof(TippyUI));
        this.RegisterHostedService(typeof(TippyProvider));

        // Data
        containerBuilder.RegisterType<Messages>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<Tips>().AsSelf().SingleInstance();

        containerBuilder.Register(
            s =>
            {
                var configurationLoaderService = s.Resolve<ConfigurationLoaderService>();
                return configurationLoaderService.GetConfiguration();
            }).SingleInstance();
    }

    public override void ConfigureServices(IServiceCollection serviceCollection)
    {
    }
}
