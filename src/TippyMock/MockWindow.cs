using DalaMock.Core.Mocks;

using Dalamud.Interface.Windowing;

namespace TippyMock;

public class MockWindow : Window
{
    private readonly MockClientState mockClientState;

    public MockWindow()
        : base("Mock Window")
    {
        this.IsOpen = true;
    }

    public override void Draw()
    {

    }
}
