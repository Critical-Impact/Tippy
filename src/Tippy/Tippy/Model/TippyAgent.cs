using System.Collections.Generic;
using System.Linq;

namespace Tippy;

public class TippyAgent
{
    private List<AnimationType>? supportedAnimations;

    public int SpriteWidth { get; set; }

    public int SpriteHeight { get; set; }

    public int SheetWidth { get; set; }

    public int SheetHeight { get; set; }

    public int PaddingWidth { get; set; }

    public int PaddingHeight { get; set; }

    public int SoundCount { get; set; }

    public Dictionary<AnimationCategory, List<AnimationType>> Animations { get; set; }

    public Dictionary<int, string> Sounds { get; set; }

    public List<AnimationType> GetAnimations(AnimationCategory category)
    {
        return this.Animations.TryGetValue(category, out var result) ? result : [];
    }

    public List<AnimationType> GetSupportedAnimations()
    {
        return this.supportedAnimations ??= this.Animations.SelectMany(c => c.Value).Distinct().ToList();
    }
}
