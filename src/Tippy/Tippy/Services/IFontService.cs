using Dalamud.Interface.ManagedFontAtlas;

namespace Tippy.Services;

public interface IFontService
{
    public IFontHandle MicrosoftSansSerifFont { get; set; }

    public IFontHandle MSSansSerifFont { get; set; }
}
