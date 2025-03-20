using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Tippy;

/// <summary>
/// Animation frame.
/// </summary>
public class AnimationFrame
{
    /// <summary>
    /// Gets or sets image coordinates in sprite sheet.
    /// </summary>
    [JsonProperty("images")]
    public int[] Images { get; set; } = null!;

    /// <summary>
    /// Gets or sets duration to show current frame.
    /// </summary>
    [JsonProperty("duration")]
    public int Duration { get; set; }

    /// <summary>
    /// Gets or sets sound to play for current frame.
    /// </summary>
    [JsonProperty("sound")]
    public int Sound { get; set; }

    [JsonProperty("exitBranch")]
    public int ExitBranch { get; set; }


    [JsonProperty("branching")]
    public AnimationFrameBranching? Branching { get; set; }
}


public class AnimationFrameBranching
{
    private static readonly Random Random = new();

    [JsonProperty("branches")]
    public List<AnimationBranch>? Branches { get; set; }

    public AnimationBranch? PickNextBranch()
    {
        if (this.Branches == null || this.Branches.Count == 0)
        {
            return null;
        }

        int randomValue = Random.Next(0, 100);

        foreach (var branch in this.Branches)
        {
            if (randomValue <= branch.Weight)
            {
                return branch;
            }

            randomValue -= branch.Weight;
        }

        return null;
    }
}

