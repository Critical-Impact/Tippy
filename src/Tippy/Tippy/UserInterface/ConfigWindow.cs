using System.Linq;
using System.Numerics;
using CheapLoc;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Tippy.Extensions;

namespace Tippy;

/// <summary>
/// Config window.
/// </summary>
public class ConfigWindow : Window
{
    private readonly TippyConfig config;
    private Tab currentTab = Tab.General;
    private bool isVisible;

    public ConfigWindow(TippyConfig config) : base("Tippy Config")
    {
        this.config = config;
        this.SizeCondition = ImGuiCond.Appearing;
        this.Size = new Vector2(300, 300);
    }

    private enum Tab
    {
        General,
        Timers,
        Blocked,
    }

    /// <summary>
    /// Draw.
    /// </summary>
    public override void Draw()
    {
        this.DrawTabs();
        switch (this.currentTab)
        {
            case Tab.General:
            {
                this.DrawGeneral();
                break;
            }

            case Tab.Timers:
            {
                this.DrawTimers();
                break;
            }

            case Tab.Blocked:
            {
                this.DrawBlocked();
                break;
            }

            default:
                this.DrawGeneral();
                break;
        }
    }

    private void DrawTabs()
    {
        if (ImGui.BeginTabBar("###Tippy_Config_TabBar", ImGuiTabBarFlags.NoTooltip))
        {
            if (ImGui.BeginTabItem(Loc.Localize("###Tippy_General_Tab", "General")))
            {
                this.currentTab = Tab.General;
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(Loc.Localize("###Tippy_Timers_Tab", "Timers")))
            {
                this.currentTab = Tab.Timers;
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(Loc.Localize("###Tippy_Blocked_Tab", "Blocked")))
            {
                this.currentTab = Tab.Blocked;
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
            ImGui.Spacing();
        }
    }

    private void DrawGeneral()
    {
        var isEnabled = this.config.IsEnabled;
        if (ImGui.Checkbox(Loc.Localize("###Tippy_IsEnabled_Checkbox", "Enable Tippy"), ref isEnabled))
        {
            this.config.IsEnabled = isEnabled;
        }

        var playSounds = this.config.IsSoundEnabled;
        if (ImGui.Checkbox(Loc.Localize("###Tippy_EnableSounds_Checkbox", "Enable Sounds"), ref playSounds))
        {
            this.config.IsSoundEnabled = playSounds;
        }

        var showIntro = this.config.ShowIntroMessages;
        if (ImGui.Checkbox(Loc.Localize("###Tippy_ShowIntro_Checkbox", "Show Intro"), ref showIntro))
        {
            this.config.ShowIntroMessages = showIntro;
        }

        var isLocked = this.config.IsLocked;
        if (ImGui.Checkbox(Loc.Localize("###Tippy_IsLocked_Checkbox", "Is Locked"), ref isLocked))
        {
            this.config.IsLocked = isLocked;
        }

        var useClassicFont = this.config.UseClassicFont;
        if (ImGui.Checkbox(Loc.Localize("###Tippy_UseClassicFont_Checkbox", "Use Classic Font"), ref useClassicFont))
        {
            this.config.UseClassicFont = useClassicFont;
        }

        var showDebugWindow = this.config.ShowDebugWindow;
        if (ImGui.Checkbox(Loc.Localize("###Tippy_ShowDebugWindow_Checkbox", "Show Debug Window"), ref showDebugWindow))
        {
            this.config.ShowDebugWindow = showDebugWindow;
        }
    }

    private void DrawTimers()
    {
        ImGui.Text(Loc.Localize("###Tippy_MessageTimeout_Slider", "Message Timeout"));
        var messageTimeout = this.config.MessageTimeout.FromMillisecondsToSeconds();
        if (ImGui.SliderInt("###MessageTimeout_Slider", ref messageTimeout, 1, 60))
        {
            this.config.MessageTimeout = messageTimeout.FromSecondsToMilliseconds();
        }

        ImGui.Spacing();

        ImGui.Text(Loc.Localize("###Tippy_TipTimeout_Slider", "Tip Timeout"));
        var tipTimeout = this.config.TipTimeout.FromMillisecondsToSeconds();
        if (ImGui.SliderInt("###TipTimeout_Slider", ref tipTimeout, 1, 60))
        {
            this.config.TipTimeout = tipTimeout.FromSecondsToMilliseconds();
        }

        ImGui.Spacing();

        ImGui.Text(Loc.Localize("###Tippy_TipCooldown_Slider", "Tip Cooldown"));
        var tipCooldown = this.config.TipCooldown.FromMillisecondsToSeconds();
        if (ImGui.SliderInt("###TipCooldown_Slider", ref tipCooldown, 0, 300))
        {
            this.config.TipCooldown = tipCooldown.FromSecondsToMilliseconds();
        }

        ImGui.Spacing();
    }

    private void DrawBlocked()
    {
        if (this.config.BannedTipIds.Count > 0)
        {
            ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("###Tippy_BlockedTips_Text", "Click on a tip to unblock."));
            ImGui.Spacing();
            foreach (var bannedTipId in this.config.BannedTipIds.ToList())
            {
                ImGui.Text(Tips.AllTips[bannedTipId].Text);
                if (ImGui.IsItemClicked())
                {
                    this.config.RemoveBannedTipId(bannedTipId);
                }
            }
        }
        else
        {
            ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("###Tippy_NoBlockedTips_Text", "You haven't blocked any tips!"));
        }
    }
}
