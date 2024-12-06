using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using CheapLoc;
using DalaMock.Host.Hosting;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Loc;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Tippy.Services;
using PluginLocalization = Dalamud.Loc.Localization;

namespace Tippy;

/// <inheritdoc />
public class TippyPlugin : HostedPlugin
{
    private readonly PluginLocalization localization;

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
        this.localization = new Localization(pluginInterface);

        var allowedLang = new [] { "de", "es", "fr", "it", "ja", "no", "pt", "ru", "zh" };

        var currentUiLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        Log.Information("Trying to set up Loc for culture {0}", currentUiLang);

        if (allowedLang.Any(x => currentUiLang == x))
        {
            var resourceFile = Assembly.GetExecutingAssembly()
                                       .GetFile($"Tippy.Tippy.Resource.translation.{currentUiLang}.json");
            if (resourceFile != null)
            {
                StreamReader streamReader = new StreamReader(resourceFile);
                var lines = streamReader.ReadToEnd();
                Loc.Setup(lines);
            }
        }
        else
        {
            Loc.SetupWithFallbacks();
        }

        this.Start();
        //this.LoadConfig();
        //TippyController = new TippyController(this);
        //this.tippyUI = new TippyUI(this);
        //this.tippyProvider = new TippyProvider(PluginInterface, new TippyAPI());

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
        containerBuilder.RegisterType<Localization>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<TextHelperService>().AsSelf().SingleInstance();

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
        serviceCollection.AddHostedService(p => p.GetRequiredService<JobMonitorService>());
        serviceCollection.AddHostedService(p => p.GetRequiredService<TippyController>());
        serviceCollection.AddHostedService(p => p.GetRequiredService<TippyUI>());
    }
}
