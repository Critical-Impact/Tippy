using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using CheapLoc;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;

namespace Tippy.Services;

public class TranslationService : IHostedService, IDisposable
{
    private readonly IPluginLog pluginLog;
    private readonly IDalamudPluginInterface pluginInterface;

    public delegate void NewLanguageLoadedDelegate();

    public event NewLanguageLoadedDelegate? OnNewLanguageLoaded;

    public TranslationService(IPluginLog pluginLog, IDalamudPluginInterface pluginInterface)
    {
        this.pluginLog = pluginLog;
        this.pluginInterface = pluginInterface;
        var currentUiLang = pluginInterface.UiLanguage;
        this.LanguageChanged(currentUiLang, false);
    }

    private void LanguageChanged(string langCode)
    {
        this.LanguageChanged(langCode, true);
    }

    private void LanguageChanged(string langCode, bool emitEvents)
    {
        this.pluginLog.Verbose("Trying to set up Loc for culture {0}", langCode);
        var allowedLang = new[] { "de", "es", "fr", "it", "ja", "no", "pt", "ru", "zh" };

        if (allowedLang.Any(x => langCode == x))
        {
            var resourceFile = typeof(TippyPlugin).Assembly
                                                  .GetManifestResourceStream($"Tippy.Tippy.Resource.translation.{langCode}.json");
            if (resourceFile != null)
            {
                StreamReader streamReader = new StreamReader(resourceFile);
                var lines = streamReader.ReadToEnd();
                Loc.Setup(lines, typeof(TippyPlugin).Assembly);
                this.pluginLog.Info($"Loaded translation files for {langCode}");
            }
            else
            {
                this.pluginLog.Warning($"Could not load translation for {langCode}, falling back to en");
                Loc.Setup("{}", typeof(TippyPlugin).Assembly);
            }
        }
        else
        {
            this.pluginLog.Warning($"No translation for {langCode}, falling back to en");
            Loc.Setup("{}", typeof(TippyPlugin).Assembly);
        }

        if (emitEvents)
        {
            this.OnNewLanguageLoaded?.Invoke();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.pluginInterface.LanguageChanged += this.LanguageChanged;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.pluginInterface.LanguageChanged -= this.LanguageChanged;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        this.pluginInterface.LanguageChanged -= this.LanguageChanged;
    }
}
