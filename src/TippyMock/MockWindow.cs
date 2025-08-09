using System.Linq;

using CheapLoc;

using DalaMock.Core.Mocks;

using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

using Dalamud.Bindings.ImGui;

using Tippy;
using Tippy.Services;

namespace TippyMock;

public class MockWindow : Window
{
    private readonly TippyController tippyController;
    private readonly TippyConfig tippyConfig;
    private readonly MockClientState mockClientState;

    public MockWindow(TippyController tippyController, TippyConfig tippyConfig)
        : base("Mock Window")
    {
        this.tippyController = tippyController;
        this.tippyConfig = tippyConfig;
        this.IsOpen = true;
    }

    public override void Draw()
    {
        var currentAgent = this.tippyConfig.CurrentAgent;

        var currentAgentName = Loc.Localize("Agent_" + currentAgent, currentAgent);

        using (var combo = ImRaii.Combo("Agent", currentAgentName))
        {
            if (combo.Success)
            {
                foreach (var agent in this.tippyController.AvailableAgents.OrderBy(c => c))
                {
                    if (ImGui.Selectable(Loc.Localize("Agent_" + agent, agent), currentAgent == agent))
                    {
                        this.tippyController.SwitchAgent(agent);
                    }
                }
            }
        }
    }
}
