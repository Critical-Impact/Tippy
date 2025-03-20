using System.Collections.Generic;

using Newtonsoft.Json;

namespace Tippy;

/// <summary>
/// Animation data.
/// </summary>
public class AnimationData
{
    /// <summary>
    /// Gets or sets animation type name.
    /// </summary>
    [JsonProperty("name")]
    public AnimationType Type { get; set; } = AnimationType.Idle;

    /// <summary>
    /// Gets or sets list of frames for animation.
    /// </summary>
    [JsonProperty("frames")]
    public List<AnimationFrame> Frames { get; set; } = null!;
}
