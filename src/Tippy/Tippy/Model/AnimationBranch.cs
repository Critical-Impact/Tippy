using Newtonsoft.Json;

namespace Tippy;

public class AnimationBranch
{
    [JsonProperty("frameIndex")]
    public int FrameIndex { get; set; }

    [JsonProperty("weight")]
    public int Weight { get; set; }
}
