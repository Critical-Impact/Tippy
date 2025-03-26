using System.IO;

using Autofac;

using DalaMock.Core.DI;
using DalaMock.Core.Mocks;
using DalaMock.Core.Windows;

namespace TippyMock;

internal class Program
{
    private static void Main(string[] args)
    {
        var dalamudConfiguration = new MockDalamudConfiguration()
        {
            GamePath = new DirectoryInfo("/home/blair/.xlcore/ffxiv/game/sqpack"),
            PluginSavePath = new DirectoryInfo("/home/blair/.xlcore/pluginConfigs"),
        };
        var mockContainer = new MockContainer(dalamudConfiguration);
        var mockDalamudUi = mockContainer.GetMockUi();
        var pluginLoader = mockContainer.GetPluginLoader();
        var mockPlugin = pluginLoader.AddPlugin(typeof(TippyPluginMock));
        //pluginLoader.StartPlugin(mockPlugin);
        mockContainer.GetContainer().Resolve<MockMockWindow>().IsOpen = true;
        mockDalamudUi.Run();
    }
}
