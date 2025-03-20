using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;

namespace Tippy.Services;

public class FontService : IFontService
{
    public IFontHandle MicrosoftSansSerifFont { get; set; }
    
    public IFontHandle MSSansSerifFont { get; set; }
    
    public FontService(IUiBuilder uiBuilder, ResourceService resourceService)
    {
        this.MicrosoftSansSerifFont = uiBuilder.FontAtlas.NewDelegateFontHandle(
            e => e.OnPreBuild(tk =>
            {
                tk.AddFontFromFile(resourceService.GetResourcePath("micross.ttf"), new SafeFontConfig() { SizePx = 14 });
            }));
        this.MSSansSerifFont = uiBuilder.FontAtlas.NewDelegateFontHandle(
            e => e.OnPreBuild(tk =>
            {
                tk.AddFontFromFile(resourceService.GetResourcePath("mssansserif.ttf"), new SafeFontConfig() { SizePx = 14 });
            }));
    }
}
