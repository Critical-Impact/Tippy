using System;
using System.Globalization;
using System.IO;
using System.Linq;
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
                Loc.Setup(lines);
                this.pluginLog.Verbose($"Loaded translation files for {langCode}");
            }
            else
            {
                this.pluginLog.Verbose($"Could not load translation for {langCode}, falling back to en");
                this.LoadFallbackTranslations();
            }
        }
        else
        {
            this.pluginLog.Verbose($"No translation for {langCode}, falling back to en");
            this.LoadFallbackTranslations();
        }

        if (emitEvents)
        {
            this.OnNewLanguageLoaded?.Invoke();
        }
    }

    private void LoadFallbackTranslations()
    {
        try
        {
            var resourceFile = typeof(TippyPlugin).Assembly
                                                  .GetManifestResourceStream("Tippy.Tippy.Resource.source.Tippy_Localizable.json");
            if (resourceFile != null)
            {
                using (StreamReader streamReader = new StreamReader(resourceFile))
                {
                    var lines = streamReader.ReadToEnd();
                    Loc.Setup(lines);
                    this.pluginLog.Verbose("Loaded fallback English translations from Tippy_Localizable.json");
                }
            }
            else
            {
                this.pluginLog.Warning("Could not load fallback translations, using CheapLoc defaults");
                Loc.SetupWithFallbacks();
            }
        }
        catch (Exception ex)
        {
            this.pluginLog.Warning(ex, "Failed to load fallback translations, using CheapLoc defaults");
            Loc.SetupWithFallbacks();
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
