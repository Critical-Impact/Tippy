using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using NAudio.Wave;
using Newtonsoft.Json;

namespace Tippy.Services;

/// <summary>
/// Tippy animation controller.
/// </summary>
public class TippyController : IHostedService, IDisposable
{
    private readonly JobMonitorService jobMonitorService;
    private readonly ResourceService resourceService;
    private readonly ITextureProvider textureProvider;
    private readonly TippyConfig tippyConfig;
    private readonly IPluginLog pluginLog;
    private readonly TextHelperService textHelperService;
    private readonly Tips tips;
    private readonly Messages messages;
    private List<AnimationData> tippyDataList;

    private string? newAgent;
    private bool? oldAgentLeft;
    private TippyFrame?[] current;
    private bool animationIsFinished;
    private Vector2 size;
    private Vector2?[] coords;
    private Vector2?[] uv0;
    private Vector2?[] uv1;
    private bool isSoundPlaying;
    private int framesCount;
    private Random random;

    public delegate void AgentSwitchedDelegate();

    public event AgentSwitchedDelegate? OnAgentSwitched;

    /// <summary>
    /// Initializes a new instance of the <see cref="TippyController"/> class.
    /// </summary>
    /// <param name="plugin">plugin.</param>
    /// <param name="textureProvider">texture provider.</param>
    /// <param name="tippyConfig">tippy's configuration.</param>
    /// <param name="pluginLog">dalamuds plugin log.</param>
    /// <param name="jobMonitorService">job monitoring service.</param>
    /// <param name="resourceService">resource service.</param>
    /// <param name="textHelperService">text helper service.</param>
    /// <param name="tips">tip data.</param>
    /// <param name="messages">message data.</param>
    public TippyController(JobMonitorService jobMonitorService, ResourceService resourceService, ITextureProvider textureProvider, TippyConfig tippyConfig, IPluginLog pluginLog, TextHelperService textHelperService, Tips tips, Messages messages)
    {
        this.jobMonitorService = jobMonitorService;
        this.resourceService = resourceService;
        this.textureProvider = textureProvider;
        this.tippyConfig = tippyConfig;
        this.pluginLog = pluginLog;
        this.textHelperService = textHelperService;
        this.tips = tips;
        this.messages = messages;
        this.random = new Random();
        this.coords = new Vector2?[5];
        this.uv0 = new Vector2?[5];
        this.uv1 = new Vector2?[5];

        this.LoadAgents();
        this.LoadAgent(this.tippyConfig.CurrentAgent, true);

        // load messages
        this.SetupMessages(true);
        this.SetupNextMessage();
        this.animationIsFinished = true;
        this.AnimationQueue.Enqueue(this.GetRandomAnimation(AnimationCategory.Arrive));
    }

    private void LoadAgents()
    {
        var agents = new List<string>();
        var resourcePath = this.resourceService.GetResourcePath(string.Empty);
        var resourceDirectory = new DirectoryInfo(resourcePath);
        foreach (var directory in resourceDirectory.EnumerateDirectories())
        {
            if (directory.GetFiles("agent.json").Length != 0)
            {
                agents.Add(directory.Name);
            }
        }

        this.AvailableAgents = agents;
    }

    private void LoadAgent(string agentName, bool initialLoad = false)
    {
        var agentPath = this.resourceService.GetResourcePath(agentName);

        var frameJson = File.ReadAllText(Path.Combine(agentPath, "frame_data.json"));
        var agentJson = File.ReadAllText(Path.Combine(agentPath, "agent.json"));
        var spriteTexturePath = Path.Combine(agentPath, "map.png");
        var bubbleTexturePath = this.resourceService.GetResourcePath("bubble.png");
        var animationData = JsonConvert.DeserializeObject<List<AnimationData>>(frameJson)!;
        var agentData = JsonConvert.DeserializeObject<TippyAgent>(agentJson)!;
        var agentSounds = new Dictionary<int, string>();
        for (var i = 1; i < agentData.SoundCount; i++)
        {
            agentSounds.Add(i, this.resourceService.GetResourcePath(Path.Join(agentName, $"sound_{i}.mp3")));
        }

        agentData.Sounds = agentSounds;
        var supportedTypes = new HashSet<AnimationType>();
        animationData = animationData.Where(c =>
        {
            if (c.Frames.Count == 0)
            {
                return false;
            }

            if (c.Frames.Count == 1 && c.Type != AnimationType.Still)
            {
                if (c.Frames[0].Images.Length == 0)
                {
                    this.pluginLog.Verbose($"{c.Type} has frame count of 0 in agent {agentName}");
                    return false;
                }

                if (c.Frames[0].Images.Length < 2 || (c.Frames[0].Images[0] == 0 && c.Frames[0].Images[1] == 0))
                {
                    this.pluginLog.Verbose($"{c.Type} has blank frames in agent {agentName} and is not still animation type.");
                    return false;
                }
            }

            supportedTypes.Add(c.Type);
            return true;
        }).ToList();

        agentData.Animations = agentData.Animations.ToDictionary(c => c.Key, c => c.Value.Where(d => supportedTypes.Contains(d)).ToList());

        this.TippyTexture = this.textureProvider.GetFromFile(spriteTexturePath);
        this.BubbleTexture = this.textureProvider.GetFromFile(bubbleTexturePath);
        this.tippyDataList = animationData;
        this.Agent = agentData;
        this.CurrentFrameIndex = 0;
        this.current = new TippyFrame[5];
        this.TippyState = TippyState.Idle;

        if (!initialLoad)
        {
            this.CloseMessage();
            this.CurrentFrameIndex = 0;
            this.AnimationQueue.Clear();
            this.AnimationQueue.Enqueue(this.GetRandomAnimation(AnimationCategory.Arrive));
            this.SetupNextAnimation();
        }
    }

    public void SwitchAgent(string agentName)
    {
        this.tippyConfig.CurrentAgent = agentName;
        this.newAgent = agentName;
    }

    /// <summary>
    /// Gets or sets get current message.
    /// </summary>
    public Message? CurrentMessage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether job changed.
    /// </summary>
    public bool JobChanged { get; set; }

    /// <summary>
    /// Gets frame timer.
    /// </summary>
    public Stopwatch FrameTimer { get; private set; } = new();

    /// <summary>
    /// Gets message timer.
    /// </summary>
    public Stopwatch MessageTimer { get; private set; } = new();

    /// <summary>
    /// Gets current frame index.
    /// </summary>
    public int CurrentFrameIndex { get; private set; }

    /// <summary>
    /// Gets last message finished timestamp.
    /// </summary>
    public DateTime? LastMessageFinished { get; private set; }

    /// <summary>
    /// Gets tip queue.
    /// </summary>
    public Queue<Tip> TipQueue { get; private set; } = new();

    /// <summary>
    /// Gets IPC Tip list.
    /// </summary>
    public List<Tip> IPCTips { get; private set; } = new();

    /// <summary>
    /// Gets message queue.
    /// </summary>
    public Queue<Message> MessageQueue { get; private set; } = new();

    /// <summary>
    /// Gets animation queue.
    /// </summary>
    public Queue<AnimationType> AnimationQueue { get; private set; } = new();

    /// <summary>
    /// Gets or sets animation type.
    /// </summary>
    public AnimationType CurrentAnimationType { get; set; }

    /// <summary>
    /// Gets or sets get tippy state.
    /// </summary>
    public TippyState TippyState { get; set; } = TippyState.NotStarted;

    /// <summary>
    /// The texture for the agent.
    /// </summary>
    public ISharedImmediateTexture TippyTexture { get; set; }

    /// <summary>
    /// The texture for the bubble that the text sits inside.
    /// </summary>
    public ISharedImmediateTexture BubbleTexture { get; set; }

    /// <summary>
    /// The active agent.
    /// </summary>
    public TippyAgent Agent { get; set; }

    /// <summary>
    /// A list of available agents.
    /// </summary>
    public List<string> AvailableAgents { get; set; }

    /// <summary>
    /// Setup tips on load or if job changes.
    /// </summary>
    /// <param name="initialLoad">indicator whether initial load.</param>
    public void SetupMessages(bool initialLoad = false)
    {
        // reset queue
        this.TipQueue.Clear();

        // add intro tips
        if (initialLoad && this.tippyConfig.ShowIntroMessages)
        {
            foreach (var message in this.messages.IntroMessages.ToArray())
            {
                this.MessageQueue.Enqueue(message);
            }
        }

        // determine role/job codes
        IEnumerable<Tip> allTips = this.tips.GeneralTips;
        if (this.jobMonitorService.CurrentJobId != 0)
        {
            var jobCode = (JobCode)this.jobMonitorService.CurrentJobId;
            var roleCode = this.jobMonitorService.CurrentRoleId is 2 or 3 ? RoleCode.DPS : (RoleCode)this.jobMonitorService.CurrentRoleId;
            allTips = allTips.Concat(this.tips.RoleTips[roleCode]).Concat(this.tips.JobTips[jobCode]);
        }

        // add tips from other plugins if any
        if (this.IPCTips.Count > 0)
        {
            allTips = allTips.Concat(this.IPCTips);
        }

        // create tip list
        var shuffledTips = this.ShuffleTips(allTips);
        foreach (var tip in shuffledTips)
        {
            if (!this.tippyConfig.BannedTipIds.Contains(tip.Id))
            {
                this.TipQueue.Enqueue(tip);
            }
        }
    }

    /// <summary>
    /// Gets indicator whether to show message.
    /// </summary>
    /// <returns>indicator whether to show message.</returns>
    public bool ShouldShowMessage()
    {
        return this.TippyState is TippyState.GivingTip or TippyState.GivingMessage;
    }

    /// <summary>
    /// Gets indicator whether can block this tip.
    /// </summary>
    /// <returns>indicator whether able to block tip..</returns>
    public bool CanBlockTip()
    {
        return this.TippyState is TippyState.GivingTip && this.CurrentMessage?.Source == MessageSource.Default;
    }

    /// <summary>
    /// Get next message now.
    /// </summary>
    public void GetMessageNow()
    {
        this.CloseMessage();
        this.LastMessageFinished = null;
    }

    /// <summary>
    /// Finish and close message.
    /// </summary>
    public void CloseMessage()
    {
        this.MessageTimer.Stop();
        this.TippyState = TippyState.Idle;
        this.LastMessageFinished = DateTime.Now;
    }

    public AnimationType GetRandomAnimation(AnimationCategory category)
    {
        var idleAnimations = this.Agent.GetAnimations(category);
        var index = this.random.Next(idleAnimations.Count);
        return idleAnimations[index];
    }

    public AnimationType GetRandomAnimation()
    {
        var idleAnimations = this.Agent.Animations.SelectMany(c => c.Value).ToList();
        var index = this.random.Next(idleAnimations.Count);
        return idleAnimations[index];
    }

    /// <summary>
    /// Play sound.
    /// </summary>
    /// <param name="num">sound to play.</param>
    public void PlaySound(int num)
    {
        if (!this.tippyConfig.IsSoundEnabled || this.isSoundPlaying) return;
        if (num == 0) return;
        this.isSoundPlaying = true;
        new Thread(() =>
        {
            WaveStream reader;
            try
            {
                if (!this.Agent.Sounds.TryGetValue(num, out var sound))
                {
                    this.pluginLog.Error($"Could not find sound {num}");
                    return;
                }
                reader = new MediaFoundationReader(sound);
            }
            catch (Exception ex)
            {
                this.isSoundPlaying = false;
                this.pluginLog.Error(ex, "Failed to create wave file reader");
                return;
            }

            using var channel = new WaveChannel32(reader)
            {
                Volume = 1f,
                PadWithZeroes = false,
            };

            using (reader)
            {
                using var output = new DirectSoundOut();

                try
                {
                    output.Init(channel);
                    output.Play();
                    this.pluginLog.Verbose($"Playing sound {num}");

                    while (output.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(500);
                    }
                }
                catch (Exception ex)
                {
                    this.isSoundPlaying = false;
                    this.pluginLog.Error(ex, "Failed to create play sound");
                    return;
                }
            }

            this.isSoundPlaying = false;
        }).Start();
    }

    /// <summary>
    /// Draw tippy animation.
    /// </summary>
    /// <returns>animation spec for imgui.</returns>
    public TippyFrame?[]? GetTippyFrame()
    {
        try
        {
            if (this.newAgent != null)
            {
                if (this.oldAgentLeft == null)
                {
                    this.oldAgentLeft = false;
                    this.CloseMessage();
                    this.FrameTimer.Restart();
                    this.CurrentFrameIndex = 0;
                    this.CurrentAnimationType = this.GetRandomAnimation(AnimationCategory.Leave);
                    this.AnimationQueue.Clear();
                    this.animationIsFinished = false;
                }

                if (this.oldAgentLeft == false && this.AnimationQueue.Count == 0 && this.animationIsFinished)
                {
                    this.oldAgentLeft = true;
                }

                if (this.oldAgentLeft == true)
                {
                    this.CurrentMessage = null;
                    this.tippyConfig.CurrentAgent = this.newAgent;
                    this.LoadAgent(this.newAgent);
                    this.OnAgentSwitched?.Invoke();
                    this.newAgent = null;
                    this.oldAgentLeft = null;
                    return null;
                }
            }
            // reset tip queue if job changed
            if (this.JobChanged)
            {
                this.SetupMessages();
                this.JobChanged = false;
            }

            // check if new message in queue to show
            if (this.MessageQueue.Count > 0 && this.TippyState != TippyState.GivingMessage)
            {
                this.CloseMessage();
                this.CurrentFrameIndex = 0;
                this.SetupNextMessage();
            }

            // check if waiting for new tip
            else if (this.TippyState == TippyState.Idle && (this.LastMessageFinished == null || DateTime.Now - this.LastMessageFinished > TimeSpan.FromMilliseconds(this.tippyConfig.TipCooldown)))
            {
                this.SetupNextMessage();
            }

            // check if timeout exceeded
            else if ((this.TippyState == TippyState.GivingMessage &&
                      this.MessageTimer.ElapsedMilliseconds > this.tippyConfig.MessageTimeout) || (this.TippyState == TippyState.GivingTip &&
                     this.MessageTimer.ElapsedMilliseconds > this.tippyConfig.TipTimeout))
            {
                this.CloseMessage();
            }

            // loop animation
            if (this.animationIsFinished && this.CurrentMessage != null) this.SetupNextAnimation(this.CurrentMessage.LoopAnimation);

            // get all frames for animation
            var selectedFrame = this.tippyDataList.FirstOrDefault(data => data.Type == this.CurrentAnimationType);
            if (selectedFrame == null)
            {
                var idleAnimations = this.Agent.GetAnimations(AnimationCategory.Idle);
                int index = this.random.Next(idleAnimations.Count);
                var nextAnimation = idleAnimations[index];
                selectedFrame = this.tippyDataList.First(data => data.Type == nextAnimation);
                this.pluginLog.Error("Could not find animation of type " + this.CurrentAnimationType);
            }

            var frames = selectedFrame.Frames;
            this.framesCount = frames.Count;

            if (this.CurrentFrameIndex >= frames.Count)
            {
                this.CurrentFrameIndex = frames.Count - 1;
            }

            // get current frame
            var frame = frames[this.CurrentFrameIndex];

            // if still within duration show last image
            if (this.FrameTimer.ElapsedMilliseconds < frame.Duration)
            {
                this.size = ImGuiHelpers.ScaledVector2(this.Agent.SpriteWidth, this.Agent.SpriteHeight);
                for (int i = 0; i < 5; i++)
                {
                    var imageStartIndex = i * 2;
                    if (frame.Images.Length >= imageStartIndex + 2 && this.uv0[i] != null)
                    {
                        this.current[i] = new TippyFrame(this.size, this.uv0[i]!.Value, this.uv1[i]!.Value, frame.Sound);
                    }
                    else
                    {
                        this.current[i] = null;
                    }
                }
                var toPlay = this.current[0];
                if (toPlay != null)
                {
                    // play sound
                    this.PlaySound(toPlay.sound);
                }
                return this.current;
            }

            // set to final if next frame is last
            if (this.CurrentFrameIndex == frames.Count - 1)
            {
                this.animationIsFinished = true;
            }

            // move to next frame
            else
            {
                if (frame.Branching != null && frame.Branching.Branches != null && frame.Branching.Branches.Count != 0)
                {
                    var nextBranch = frame.Branching.PickNextBranch();
                    if (nextBranch != null)
                    {
                        this.CurrentFrameIndex = nextBranch.FrameIndex;
                        this.FrameTimer.Restart();
                    }
                    else
                    {
                        this.CurrentFrameIndex += 1;
                        this.FrameTimer.Restart();
                    }
                }
                else
                {
                    this.CurrentFrameIndex += 1;
                    this.FrameTimer.Restart();
                }
            }

            // update frame parameters
            this.size = ImGuiHelpers.ScaledVector2(this.Agent.SpriteWidth, this.Agent.SpriteHeight);
            for (int i = 0; i < 5; i++)
            {
                var imageStartIndex = i * 2;
                if (frame.Images.Length >= imageStartIndex + 2)
                {
                    this.coords[i] = new Vector2(frame.Images[imageStartIndex], frame.Images[imageStartIndex + 1]);
                    this.uv0[i] = this.ToSpriteSheetScale(this.coords[i]!.Value);
                    this.uv1[i] = this.ToSpriteSheetScale(this.coords[i]!.Value + new Vector2(this.Agent.SpriteWidth, this.Agent.SpriteHeight));
                    this.current[i] = new TippyFrame(this.size, this.uv0[i]!.Value, this.uv1[i]!.Value, frame.Sound);
                }
                else if (frames.Count == this.CurrentFrameIndex + 1 && frame.Images.Length == 0)
                {
                    //Some animations have a final frame with no images, use the previous frame's image if that's the case
                }
                else
                {
                    this.current[i] = null;
                    this.coords[i] = null;
                    this.uv0[i] = null;
                    this.uv1[i] = null;
                }
            }

            var toPlay2 = this.current[0];
            if (toPlay2 != null)
            {
                // play sound
                this.PlaySound(toPlay2.sound);
            }

            // return animation
            return this.current;
        }
        catch (Exception)
        {
            // show previous frame in case something went wrong
            this.pluginLog.Verbose("Failed frame at index " + this.CurrentFrameIndex + "/" + this.framesCount + " for " + this.CurrentAnimationType);
            this.CurrentFrameIndex = 0;
            this.animationIsFinished = true;
            return this.current;
        }
    }

    /// <summary>
    /// Dispose animator.
    /// </summary>
    public void Dispose()
    {
        try
        {
            var count = 0;
            while (this.isSoundPlaying && count < 20)
            {
                Thread.Sleep(100);
                count++;
            }

            this.isSoundPlaying = true;
            this.MessageQueue.Clear();
            this.CloseMessage();
            this.FrameTimer.Stop();

            this.tips.OnTipsChanged -= this.TipsOnOnTipsChanged;
        }
        catch (Exception ex)
        {
            this.pluginLog.Error(ex, "Failed to dispose tippy controller");
        }
    }

    /// <summary>
    /// Add priority tip by message.
    /// </summary>
    /// <param name="message">message.</param>
    /// <param name="animationType">animation type.</param>
    public void AddMessage(string message, AnimationType animationType)
    {
        this.MessageQueue.Enqueue(new Message(message, animationType));
    }

    /// <summary>
    /// Add tip by text.
    /// </summary>
    /// <param name="text">message.</param>
    /// <param name="messageSource">message source.</param>
    /// <returns>indicator whether successful.</returns>
    public bool AddTip(string text, MessageSource messageSource)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var tip = new Tip(text)
        {
            Source = messageSource,
        };
        this.IPCTips.Add(tip);
        this.SetupMessages();
        return true;
    }

    /// <summary>
    /// Add message by text.
    /// </summary>
    /// <param name="text">message.</param>
    /// <param name="messageSource">message source.</param>
    /// <returns>indicator whether successful.</returns>
    public bool AddMessage(string text, MessageSource messageSource)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var message = new Message(text)
        {
            Source = messageSource,
        };
        this.MessageQueue.Enqueue(message);
        return true;
    }

    /// <summary>
    /// Block message.
    /// </summary>
    public void BlockTip()
    {
        if (this.TippyState == TippyState.GivingTip && this.CurrentMessage != null)
        {
            this.tippyConfig.AddBannedTipId(this.CurrentMessage.Id);
            this.CloseMessage();
        }
    }

    /// <summary>
    /// Debug message.
    /// </summary>
    /// <param name="animationType">animation.</param>
    public void DebugMessage(AnimationType animationType)
    {
        this.MessageQueue.Clear();
        this.CloseMessage();
        var msg = new Message("Hello! This is a message from Tippy. Please enjoy.", animationType)
        {
            LoopAnimation = true,
        };
        this.MessageQueue.Enqueue(msg);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.FrameTimer.Start();
        this.tips.OnTipsChanged += this.TipsOnOnTipsChanged;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.FrameTimer.Stop();
        this.tips.OnTipsChanged -= this.TipsOnOnTipsChanged;
        return Task.CompletedTask;
    }

    private void TipsOnOnTipsChanged()
    {
        this.SetupMessages();
    }

    private IEnumerable<Tip> ShuffleTips(IEnumerable<Tip> tips)
    {
        var rnd = new Random();
        return tips.OrderBy(_ => rnd.Next()).ToArray();
    }

    private void SetupNextMessage()
    {
        while (true)
        {
            if (this.MessageQueue.Count != 0)
            {
                this.MessageQueue.TryDequeue(out var message);
                if (message != null) this.SetMessage(message, TippyState.GivingMessage);
            }
            else if (this.TipQueue.Count != 0)
            {
                this.TipQueue.TryDequeue(out var tip);
                if (tip != null) this.SetMessage(tip, TippyState.GivingTip);
            }
            else
            {
                this.SetupMessages();
                continue;
            }

            break;
        }
    }

    private void SetMessage(Message message, TippyState tippyState)
    {
        message.Text = this.textHelperService.SanitizeText(message.Text);
        message.Text = this.textHelperService.WordWrap(message.Text, 30);
        this.CurrentMessage = message;
        this.CurrentFrameIndex = 0;
        if (this.AnimationQueue.Count > 0)
        {
            this.AnimationQueue.TryDequeue(out var animationType);
            this.CurrentAnimationType = animationType;
        }
        else
        {
            if (message.AnimationType != null)
            {
                this.CurrentAnimationType = message.AnimationType.Value;
            }
            else if (message.AnimationCategory != null)
            {
                this.CurrentAnimationType = this.GetRandomAnimation(message.AnimationCategory.Value);
            }
            else
            {
                this.CurrentAnimationType = this.GetRandomAnimation();
            }
        }

        this.TippyState = tippyState;
        this.MessageTimer.Restart();
    }

    private void SetupNextAnimation(bool loop = false)
    {
        if (this.AnimationQueue.Count > 0)
        {
            this.AnimationQueue.TryDequeue(out var animationType);
            this.CurrentAnimationType = animationType;
        }
        else if (!loop)
        {
            this.CurrentAnimationType = this.GetIdleAnimation();
        }

        this.animationIsFinished = false;
        this.CurrentFrameIndex = 0;
        this.FrameTimer.Restart();
    }

    private Vector2 ToSpriteSheetScale(Vector2 input) => new(input.X / this.Agent.SheetWidth, input.Y / this.Agent.SheetHeight);

    private AnimationType GetIdleAnimation()
    {
        var rand = this.random.Next(0, 100);
        if (rand < 80)
        {
            return this.GetRandomAnimation(AnimationCategory.Still);
        }

        return this.GetRandomAnimation(AnimationCategory.Idle);
    }
}
