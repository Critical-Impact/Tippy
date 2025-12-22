using System;
using System.Threading;
using System.Threading.Tasks;

using DalaMock.Shared.Interfaces;

using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility.Raii;

using Dalamud.Bindings.ImGui;

using Tippy.Services;

namespace TippyMock;

public class MockFontService : IFontService
{
    public MockFontService(IFont font)
    {
        this.MicrosoftSansSerifFont = new MockFontHandle(font.DefaultFont);
        this.MSSansSerifFont = new MockFontHandle(font.DefaultFont);
    }

    public IFontHandle MicrosoftSansSerifFont { get; set; }

    public IFontHandle MSSansSerifFont { get; set; }
}

public class MockFontHandle : IFontHandle
{
    private readonly ImFontPtr fontPtr;
    private IDisposable? pushedFont;

    public MockFontHandle(ImFontPtr fontPtr)
    {
        this.fontPtr = fontPtr;
    }

    public void Dispose()
    {
    }
    
    public ILockedImFont? TryLock(out string? errorMessage)
    {
        throw new NotImplementedException();
    }
    
    public ILockedImFont Lock()
    {
       throw new NotImplementedException();
    }

    public IDisposable Push()
    {
        if (this.pushedFont == null)
        {
            this.pushedFont = ImRaii.PushFont(this.fontPtr);
        }

        return this.pushedFont;
    }

    public void Pop()
    {
        this.pushedFont?.Dispose();
    }

    public Task<IFontHandle> WaitAsync()
    {
        throw new NotImplementedException();
    }
    
    public Task<IFontHandle> WaitAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    public Exception? LoadException => null;

    public bool Available => true;

    public event IFontHandle.ImFontChangedDelegate? ImFontChanged;
}
