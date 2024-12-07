using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CheapLoc;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;

namespace Tippy.Services;

public class TranslationService : IHostedService
{
    public TranslationService(IPluginLog pluginLog)
    {
        var allowedLang = new[] { "de", "es", "fr", "it", "ja", "no", "pt", "ru", "zh" };

        var currentUiLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        pluginLog.Verbose("Trying to set up Loc for culture {0}", currentUiLang);

        if (allowedLang.Any(x => currentUiLang == x))
        {
            var resourceFile = typeof(TippyPlugin).Assembly
                                                  .GetManifestResourceStream($"Tippy.Tippy.Resource.translation.{currentUiLang}.json");
            if (resourceFile != null)
            {
                StreamReader streamReader = new StreamReader(resourceFile);
                var lines = streamReader.ReadToEnd();
                Loc.Setup(lines);
                pluginLog.Verbose($"Loaded translation files for {currentUiLang}");
            }
            else
            {
                pluginLog.Verbose($"Could not load translation for {currentUiLang}, falling back to en");
                Loc.SetupWithFallbacks();
            }
        }
        else
        {
            pluginLog.Verbose($"No translation for {currentUiLang}, falling back to en");
            Loc.SetupWithFallbacks();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
