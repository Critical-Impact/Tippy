using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

using CheapLoc;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Microsoft.Extensions.Hosting;
using Tippy.Services;

namespace Tippy;

/// <summary>
/// Tippy character.
/// </summary>
public class TippyUI : IHostedService, IDisposable
{
    /// <summary>
    /// Config window.
    /// </summary>
    private readonly ConfigWindow configWindow;

    private readonly TippyConfig config;

    private readonly IUiBuilder uiBuilder;
    private readonly TippyController tippyController;
    private readonly IPluginLog pluginLog;
    private readonly JobMonitorService jobMonitorService;
    private readonly IEnumerable<Window> windows;
    private readonly IWindowSystem windowSystem;
    private readonly IFontService fontService;

    private string[] debugAnimationNames;

    private Vector2 windowSize;
    private Vector2 bubbleSize;
    private Vector2 bubbleSpeechOffset;
    private Vector2 buttonSize;
    private Vector2 bubbleButtonOffset;
    private Vector2 tippyOffset;
    private Vector2 agentPos;
    private Vector2 bubblePos;
    private int debugSelectedAnimationIndex;
    private string debugMessageText = string.Empty;
    private string debugTipText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TippyUI"/> class.
    /// </summary>
    /// <param name="plugin">plugin.</param>
    public TippyUI(
        ResourceService resourceService,
        ConfigWindow configWindow,
        TippyConfig config,
        IUiBuilder uiBuilder,
        TippyController tippyController,
        IPluginLog pluginLog,
        JobMonitorService jobMonitorService,
        IEnumerable<Window> windows,
        IWindowSystem windowSystem,
        IFontService fontService)
    {
        this.configWindow = configWindow;
        this.config = config;
        this.uiBuilder = uiBuilder;
        this.tippyController = tippyController;
        this.pluginLog = pluginLog;
        this.jobMonitorService = jobMonitorService;
        this.windows = windows;
        this.windowSystem = windowSystem;
        this.fontService = fontService;
        this.tippyController.OnAgentSwitched += this.AgentSwitched;
        this.AgentSwitched();
    }

    private void AgentSwitched()
    {
        this.debugSelectedAnimationIndex = 0;
        this.debugAnimationNames = this.tippyController.Agent.GetSupportedAnimations().Select(c => c.ToString()).OrderBy(c => c).ToArray();
    }

    private IDisposable? PushedFont { get; set; }

    /// <summary>
    /// Get button (window) flags.
    /// </summary>
    /// <returns>window flags.</returns>
    public static ImGuiWindowFlags GetButtonFlags()
    {
        return ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize |
               ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoScrollbar |
               ImGuiWindowFlags.NoScrollWithMouse;
    }

    /// <summary>
    /// Get window flags.
    /// </summary>
    /// <returns>window flags.</returns>
    public ImGuiWindowFlags GetWindowFlags()
    {
        var imGuiWindowFlags = ImGuiWindowFlags.NoTitleBar |
                               ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoFocusOnAppearing |
                               ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoScrollbar;
        if (this.config.IsLocked)
        {
            imGuiWindowFlags |= ImGuiWindowFlags.NoMove;
        }

        return imGuiWindowFlags;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var pluginWindow in this.windows)
        {
            this.windowSystem.AddWindow(pluginWindow);
        }

        this.uiBuilder.Draw += this.Draw;
        this.uiBuilder.OpenConfigUi += this.OnOpenConfigUi;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.uiBuilder.Draw -= this.Draw;
        this.uiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Dispose.
    /// </summary>
    public void Dispose()
    {
        this.uiBuilder.Draw -= this.Draw;
        this.uiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
        this.tippyController.OnAgentSwitched -= this.AgentSwitched;
    }

    private void Draw()
    {
        try
        {
            this.windowSystem.Draw();
            if (!this.config.IsEnabled) return;
            this.CalcUI();
            this.StartContainer();
            if (this.tippyController.ShouldShowMessage())
            {
                this.DrawBubble();
                this.DrawBubbleButton();
            }

            this.DrawTippy();
            this.EndContainer();
            this.DrawDebug();
        }
        catch (Exception ex)
        {
            this.pluginLog.Error(ex, "Fools exception OnDraw caught.");
        }
    }

    private void CalcUI()
    {
        // adjust some values for dalamud scaling
        var bubbleHeight = 155;
        var bubbleWidth = 200;

        var spriteWidth = this.tippyController.Agent.SpriteWidth;
        var spriteHeight = this.tippyController.Agent.SpriteHeight;

        var paddingWidth = this.tippyController.Agent.PaddingWidth;
        var paddingHeight = this.tippyController.Agent.PaddingHeight;

        var windowWidth = Math.Max(bubbleWidth, spriteWidth) + paddingWidth;

        var windowHeight = bubbleHeight + spriteHeight + paddingHeight;

        var bubbleSpeechOffsetVal = 10;
        if (ImGuiHelpers.GlobalScale is > 1 and < 2)
        {
            bubbleHeight += 10;
            bubbleSpeechOffsetVal -= 5;
        }

        // set size / pos
        this.windowSize = ImGuiHelpers.ScaledVector2(windowWidth, windowHeight);
        this.bubbleSize = ImGuiHelpers.ScaledVector2(bubbleWidth, bubbleHeight);
        this.agentPos = ImGuiHelpers.ScaledVector2((this.windowSize.X - this.tippyController.Agent.SpriteWidth) / 2.0f, this.windowSize.Y - this.tippyController.Agent.SpriteHeight);
        this.bubblePos = ImGuiHelpers.ScaledVector2((this.windowSize.X - bubbleWidth) / 2.0f, paddingHeight);
        this.bubbleSpeechOffset = ImGuiHelpers.ScaledVector2(bubbleSpeechOffsetVal, bubbleSpeechOffsetVal);
        this.bubbleButtonOffset = ImGuiHelpers.ScaledVector2(12, 25);
        this.buttonSize = ImGuiHelpers.ScaledVector2(40, 22);
    }

    private void StartContainer()
    {
        ImGui.SetNextWindowSize(this.windowSize, ImGuiCond.Always);
        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize - this.windowSize, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.Border, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, ImGuiColors.DalamudViolet);
        ImGui.Begin("###TippyWindow", this.GetWindowFlags());
        if (this.config.UseClassicFont)
        {
            this.PushedFont = this.fontService.MSSansSerifFont.Push();
        }
        else
        {
            this.PushedFont = this.fontService.MicrosoftSansSerifFont.Push();
        }

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0, 0, 1));
    }

    private void EndContainer()
    {
        ImGui.PopStyleColor(1);
        if (this.PushedFont != null)
        {
            this.PushedFont.Dispose();
            this.PushedFont = null;
        }
        ImGui.End();
        ImGui.PopStyleColor(3);
    }

    private void DrawBubble()
    {
        ImGui.SetCursorPos(this.bubblePos);
        ImGui.Image(this.tippyController.BubbleTexture.GetWrapOrEmpty().ImGuiHandle, this.bubbleSize);
        ImGui.SetCursorPos(this.bubblePos + this.bubbleSpeechOffset);
        ImGui.TextUnformatted(this.tippyController.CurrentMessage?.Text ?? string.Empty);
    }

    private void DrawBubbleButton()
    {
        ImGui.SetCursorPos(this.bubblePos + this.bubbleSize - this.buttonSize - this.bubbleButtonOffset);
        ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGuiColors.DalamudGrey3);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGuiColors.DalamudGrey);
        ImGui.PushStyleColor(ImGuiCol.Border, ImGuiColors.DalamudGrey3);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.2f);

        if (ImGui.Button("OK", this.buttonSize))
        {
            this.tippyController.CloseMessage();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(4);
    }

    private void DrawTippy()
    {
        ImGui.BeginGroup();
        ImGui.SetCursorPos(this.agentPos);

        var frameSpec = this.tippyController.GetTippyFrame();
        if (frameSpec != null)
        {
            foreach (var frame in frameSpec)
            {
                if (frame != null)
                {
                    ImGui.SetCursorPos(this.agentPos);
                    if (this.tippyController.TippyTexture.TryGetWrap(out var texture, out _))
                    {
                        ImGui.Image(
                            texture.ImGuiHandle,
                            frame.size,
                            frame.uv0,
                            frame.uv1);
                        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                        {
                            this.tippyController.AnimationQueue.Enqueue(this.tippyController.GetRandomAnimation(AnimationCategory.Random));
                        }
                    }
                }
            }
        }

        ImGui.EndGroup();
        this.DrawTippyMenu();
    }

    private void DrawTippyMenu()
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("###Tippy_Tippy_Popup");
        }

        if (ImGui.BeginPopup("###Tippy_Tippy_Popup"))
        {
            if (ImGui.MenuItem(Loc.Localize("###Tippy_ShowNewTip_MenuItem", "Show new tip")))
            {
                this.tippyController.GetMessageNow();
            }

            if (this.tippyController.CanBlockTip())
            {
                if (ImGui.MenuItem(Loc.Localize("###Tippy_BlockTip_MenuItem", "Don't show tip again")))
                {
                    this.tippyController.BlockTip();
                }
            }

            if (!this.configWindow.IsOpen)
            {
                if (ImGui.MenuItem(Loc.Localize("###Tippy_OpenSettings_MenuItem", "Open settings")))
                {
                    this.OnOpenConfigUi();
                }
            }

            ImGui.EndPopup();
        }
    }

    private void DrawDebug()
    {
        if (this.config.ShowDebugWindow)
        {
            ImGui.SetNextWindowSize(ImGuiHelpers.ScaledVector2(260, 350), ImGuiCond.Appearing);
            ImGui.Begin("Tippy Debug Window");
            ImGui.Text($"State: {this.tippyController.TippyState}");
            ImGui.Text($"JobId: {this.jobMonitorService.CurrentJobId}");
            ImGui.Text($"RoleId: {this.jobMonitorService.CurrentRoleId}");
            ImGui.Text($"Tip Queue: {this.tippyController.TipQueue.Count}");
            ImGui.Text($"Msg Queue: {this.tippyController.MessageQueue.Count}");
            ImGui.Text($"Anim Queue: {this.tippyController.AnimationQueue.Count}");
            ImGui.Text($"Last Tip: {this.tippyController.LastMessageFinished}");
            ImGui.Text($"Animation: {this.tippyController.CurrentAnimationType}");
            ImGui.Text($"Frame Index: {this.tippyController.CurrentFrameIndex}");
            ImGui.Text($"Elapsed Time: {this.tippyController.MessageTimer.ElapsedMilliseconds / 1000} s");
            ImGuiHelpers.ScaledDummy(5f);
            ImGui.SetNextItemWidth(150f * ImGuiHelpers.GlobalScale);
            ImGui.Combo("####Animation", ref this.debugSelectedAnimationIndex, this.debugAnimationNames, this.debugAnimationNames.Length);
            ImGui.SameLine();
            if (ImGui.SmallButton("Add"))
            {
                this.tippyController.DebugMessage(Enum.Parse<AnimationType>(this.debugAnimationNames[this.debugSelectedAnimationIndex]));
            }

            ImGui.InputTextWithHint("###SendMessage", "Message Text", ref this.debugMessageText, 200);
            ImGui.SameLine();
            if (ImGui.SmallButton("Send###Message"))
            {
                this.tippyController.CloseMessage();
                this.tippyController.AddMessage(this.debugMessageText, MessageSource.Debug);
                this.debugMessageText = string.Empty;
            }

            ImGui.InputTextWithHint("###TipText", "Tip Text", ref this.debugTipText, 200);
            ImGui.SameLine();
            if (ImGui.SmallButton("Send###Tip"))
            {
                this.tippyController.AddTip(this.debugTipText, MessageSource.Debug);
                this.debugTipText = string.Empty;
            }

            ImGui.End();
        }
    }

    private void OnOpenConfigUi()
    {
        this.configWindow.IsOpen ^= true;
    }
}
