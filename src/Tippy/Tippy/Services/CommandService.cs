using System;

using CheapLoc;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Tippy.Services;

public class CommandService : IDisposable
{
    private readonly TippyConfig tippyConfig;
    private readonly IDalamudPluginInterface pluginInterface;
    private readonly ICommandManager commandManager;
    private readonly TippyUI tippyUi;
    private readonly TippyController tippyController;
    private readonly IPluginLog pluginLog;
    private readonly ConfigWindow configWindow;

    public CommandService(TippyConfig tippyConfig, IDalamudPluginInterface pluginInterface, ICommandManager commandManager, TippyUI tippyUi, TippyController tippyController, IPluginLog pluginLog, ConfigWindow configWindow)
    {
        this.tippyConfig = tippyConfig;
        this.pluginInterface = pluginInterface;
        this.commandManager = commandManager;
        this.tippyUi = tippyUi;
        this.tippyController = tippyController;
        this.pluginLog = pluginLog;
        this.configWindow = configWindow;
        commandManager.AddHandler("/tippy", new CommandInfo(this.ToggleTippy)
        {
            HelpMessage = Loc.Localize("Tippy_Toggle_Command", "Show Tippy."),
            ShowInHelp = true,
        });
        commandManager.AddHandler("/tippyconfig", new CommandInfo(this.ToggleTippyConfig)
        {
            HelpMessage = Loc.Localize("Tippy_Config_Command", "Show Tippy config/settings."),
            ShowInHelp = true,
        });
        commandManager.AddHandler("/tippysendmsg", new CommandInfo(this.SendMessage)
        {
            HelpMessage = Loc.Localize("Tippy_Message_Command", "Send a message for Tippy to show (usually) right away."),
            ShowInHelp = true,
        });
        commandManager.AddHandler("/tippysendtip", new CommandInfo(this.SendTip)
        {
            HelpMessage = Loc.Localize("Tippy_Tip_Command", "Send a tip for Tippy to show later at random."),
            ShowInHelp = true,
        });
    }

    public void Dispose()
    {
        this.commandManager.RemoveHandler("/tippy");
        this.commandManager.RemoveHandler("/tippyconfig");
        this.commandManager.RemoveHandler("/tippysendmsg");
        this.commandManager.RemoveHandler("/tippysendtip");
    }

    private void ToggleTippyConfig(string command, string arguments)
    {
        this.configWindow.IsOpen ^= true;
    }

    private void ToggleTippy(string command, string arguments)
    {
        this.tippyConfig.IsEnabled ^= true;
        this.pluginInterface.SavePluginConfig(this.tippyConfig);
    }

    private void SendMessage(string command, string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            arguments = Loc.Localize("Tippy_MsgHelp_Command", "You need to send the message after /tippysendmsg. Like /tippysendmsg I love you Tippy.");
        }

        this.tippyController.CloseMessage();
        var result = this.tippyController.AddMessage(arguments, MessageSource.User);
        if (!result) this.pluginLog.Info("Failed to send Tippy Tip.");
    }

    private void SendTip(string command, string arguments)
    {
        bool result;
        if (string.IsNullOrWhiteSpace(arguments))
        {
            arguments = Loc.Localize("Tippy_TipHelp_Command", "You need to send the tip after /tippysendtip. Like /tippysendtip I love you Tippy.");
            this.tippyController.CloseMessage();
            result = this.tippyController.AddMessage(arguments, MessageSource.User);
        }
        else
        {
            this.tippyController.CloseMessage();
            result = this.tippyController.AddTip(arguments, MessageSource.User);
        }

        if (!result) this.pluginLog.Info("Failed to send Tippy Message.");
    }
}
