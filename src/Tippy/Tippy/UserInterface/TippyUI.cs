using System;
using System.Collections.Generic;
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
    private readonly ResourceService resourceService;
    private readonly ConfigWindow configWindow;

    /// <summary>
    /// Config window.
    /// </summary>
    public readonly ConfigWindow ConfigWindow;

    private readonly TippyConfig config;

    private readonly ITextureProvider textureProvider;
    private readonly IUiBuilder uiBuilder;
    private readonly TippyController tippyController;
    private readonly IPluginLog pluginLog;
    private readonly JobMonitorService jobMonitorService;
    private readonly IEnumerable<Window> windows;
    private readonly IWindowSystem windowSystem;

    private readonly TippyPlugin plugin;
    private readonly ISharedImmediateTexture tippyTexture;
    private readonly ISharedImmediateTexture bubbleTexture;
    private readonly string[] debugAnimationNames;

    private Vector2 windowSize;
    private Vector2 contentPos;
    private Vector2 bubbleSize;
    private Vector2 bubbleOffset;
    private Vector2 bubbleSpeechOffset;
    private Vector2 buttonSize;
    private Vector2 bubbleButtonOffset;
    private Vector2 tippyOffset;
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
        ITextureProvider textureProvider,
        IUiBuilder uiBuilder,
        TippyController tippyController,
        IPluginLog pluginLog,
        JobMonitorService jobMonitorService,
        IEnumerable<Window> windows,
        IWindowSystem windowSystem)
    {
        this.resourceService = resourceService;
        this.ConfigWindow = configWindow;
        this.config = config;
        this.textureProvider = textureProvider;
        this.uiBuilder = uiBuilder;
        this.tippyController = tippyController;
        this.pluginLog = pluginLog;
        this.jobMonitorService = jobMonitorService;
        this.windows = windows;
        this.windowSystem = windowSystem;
        this.debugAnimationNames = Enum.GetNames(typeof(AnimationType));
        var spriteTexturePath = this.resourceService.GetResourcePath("map.png");
        this.tippyTexture = this.textureProvider.GetFromFile(spriteTexturePath);
        var bubbleTexturePath = this.resourceService.GetResourcePath("bubble.png");
        this.bubbleTexture = this.textureProvider.GetFromFile(bubbleTexturePath);
        MicrosoftSansSerifFont = uiBuilder.DefaultFontHandle;
        MSSansSerifFont = uiBuilder.DefaultFontHandle;
        // this.MicrosoftSansSerifFont = this.uiBuilder.FontAtlas.NewDelegateFontHandle(
        //     e => e.OnPreBuild(tk =>
        //     {
        //         tk.AddFontFromFile(this.resourceService.GetResourcePath("micross.ttf"), new SafeFontConfig() { SizePx = 14 });
        //     }));
        // this.MSSansSerifFont = this.uiBuilder.FontAtlas.NewDelegateFontHandle(
        //     e => e.OnPreBuild(tk =>
        //     {
        //         tk.AddFontFromFile(this.resourceService.GetResourcePath("mssansserif.ttf"), new SafeFontConfig() { SizePx = 14 });
        //     }));
    }

    private IFontHandle MicrosoftSansSerifFont { get; set; }

    private IFontHandle MSSansSerifFont { get; set; }

    private IDisposable? PushedFont { get; set; }

    /// <summary>
    /// Get window flags.
    /// </summary>
    /// <returns>window flags.</returns>
    public ImGuiWindowFlags GetWindowFlags()
    {
        var imGuiWindowFlags = ImGuiWindowFlags.NoTitleBar |
                               ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoFocusOnAppearing |
                               ImGuiWindowFlags.NoBringToFrontOnFocus;
        if (this.config.IsLocked)
        {
            imGuiWindowFlags |= ImGuiWindowFlags.NoMove;
        }

        return imGuiWindowFlags;
    }

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
    /// Dispose.
    /// </summary>
    public void Dispose()
    {
        this.uiBuilder.Draw -= this.Draw;
        this.uiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
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
        var bubbleSpeechOffsetVal = 10;
        if (ImGuiHelpers.GlobalScale is > 1 and < 2)
        {
            bubbleHeight += 10;
            bubbleSpeechOffsetVal -= 5;
        }

        // set size / pos
        this.windowSize = ImGuiHelpers.ScaledVector2(220, 280);
        this.contentPos = ImGuiHelpers.ScaledVector2(120, 105);
        this.bubbleSize = ImGuiHelpers.ScaledVector2(200, bubbleHeight);
        this.bubbleOffset = ImGuiHelpers.ScaledVector2(85, bubbleHeight);
        this.bubbleSpeechOffset = ImGuiHelpers.ScaledVector2(bubbleSpeechOffsetVal, bubbleSpeechOffsetVal);
        this.bubbleButtonOffset = ImGuiHelpers.ScaledVector2(64, -50);
        this.buttonSize = ImGuiHelpers.ScaledVector2(40, 22);
        this.tippyOffset = ImGuiHelpers.ScaledVector2(20, 0);
    }

    private void StartContainer()
    {
        ImGui.SetNextWindowSize(this.windowSize, ImGuiCond.Always);
        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize - this.windowSize, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.Border, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, ImGuiColors.DalamudViolet);
        ImGui.Begin("###TippyWindow", GetWindowFlags());
        if (this.config.UseClassicFont)
        {
            this.PushedFont = this.MSSansSerifFont.Push();
        }
        else
        {
            this.PushedFont = this.MicrosoftSansSerifFont.Push();
        }

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0, 0, 1));
    }

    private void EndContainer()
    {
        ImGui.End();
        ImGui.PopStyleColor(4);
        if (this.PushedFont != null)
        {
            this.PushedFont.Dispose();
            this.PushedFont = null;
        }
    }

    private void DrawBubble()
    {
        ImGui.SetCursorPos(ImGui.GetWindowSize() - this.contentPos - this.bubbleOffset);
        ImGui.Image(this.bubbleTexture.GetWrapOrEmpty().ImGuiHandle, this.bubbleSize);
        ImGui.SetCursorPos(ImGui.GetWindowSize() - this.contentPos - this.bubbleOffset + this.bubbleSpeechOffset);
        ImGui.TextUnformatted(this.tippyController.CurrentMessage.Text);
    }

    private void DrawBubbleButton()
    {
        ImGui.SetCursorPos(ImGui.GetWindowSize() - this.contentPos + this.bubbleButtonOffset);
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
        ImGui.SetCursorPos(ImGui.GetWindowSize() - this.contentPos - this.tippyOffset);
        var frameSpec = this.tippyController.GetTippyFrame();
        ImGui.Image(this.tippyTexture.GetWrapOrEmpty().ImGuiHandle, frameSpec.size, frameSpec.uv0, frameSpec.uv1);
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

            if (!this.ConfigWindow.IsOpen)
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
                this.tippyController.DebugMessage((AnimationType)this.debugSelectedAnimationIndex);
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
        this.ConfigWindow.IsOpen ^= true;
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
}
