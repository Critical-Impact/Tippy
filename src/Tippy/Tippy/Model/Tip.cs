namespace Tippy;

/// <summary>
/// Tippy Tip.
/// </summary>
public class Tip : Message
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Tip"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="text">tip text.</param>
    public Tip(string text)
        : base(text)
    {
        this.TipType = TipType.General;
        this.JobCode = JobCode.NONE;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tip"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="text">tip text.</param>
    /// <param name="animationType">animation type.</param>
    public Tip(string text, AnimationType animationType)
        : base(text, animationType)
    {
        this.TipType = TipType.General;
        this.JobCode = JobCode.NONE;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tip"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="text">tip text.</param>
    /// <param name="animationCategory">animation category.</param>
    public Tip(string text, AnimationCategory animationCategory)
        : base(text, animationCategory)
    {
        this.TipType = TipType.General;
        this.JobCode = JobCode.NONE;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tip"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="id">id of tip.</param>
    /// <param name="text">tip text.</param>
    public Tip(string id, string text)
        : base(id, text)
    {
        this.TipType = TipType.General;
        this.JobCode = JobCode.NONE;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tip"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="id">id of tip.</param>
    /// <param name="text">tip text.</param>
    /// <param name="animationType">animation type.</param>
    public Tip(string id, string text, AnimationType animationType)
        : base(id, text)
    {
        this.TipType = TipType.General;
        this.JobCode = JobCode.NONE;
        this.AnimationType = animationType;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Tip"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="id">id of tip.</param>
    /// <param name="text">tip text.</param>
    /// <param name="animationCategory">animation category.</param>
    public Tip(string id, string text, AnimationCategory animationCategory)
        : base(id, text)
    {
        this.TipType = TipType.General;
        this.JobCode = JobCode.NONE;
        this.AnimationCategory = animationCategory;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tip"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="id">id of tip.</param>
    /// <param name="jobCode">job id of the tip.</param>
    /// <param name="text">tip text.</param>
    public Tip(string id, JobCode jobCode, string text)
        : base(id, text)
    {
        this.TipType = TipType.Job;
        this.JobCode = jobCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tip"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="id">id of tip.</param>
    /// <param name="roleCode">role id of the tip.</param>
    /// <param name="text">tip text.</param>
    public Tip(string id, RoleCode roleCode, string text)
        : base(id, text)
    {
        this.TipType = TipType.Job;
        this.RoleCode = roleCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tip"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="id">id of tip.</param>
    /// <param name="jobCode">job id of the tip.</param>
    /// <param name="text">tip text.</param>
    /// <param name="animationType">animation type.</param>
    public Tip(string id, JobCode jobCode, string text, AnimationType animationType)
        : base(id, text)
    {
        this.TipType = TipType.Job;
        this.JobCode = jobCode;
        this.AnimationType = animationType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tip"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="id">id of tip.</param>
    /// <param name="jobCode">job id of the tip.</param>
    /// <param name="text">tip text.</param>
    /// <param name="animationCategory">animation category.</param>
    public Tip(string id, JobCode jobCode, string text, AnimationCategory animationCategory)
        : base(id, text)
    {
        this.TipType = TipType.Job;
        this.JobCode = jobCode;
        this.AnimationCategory = animationCategory;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tip"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="id">id of tip.</param>
    /// <param name="roleCode">role id of the tip.</param>
    /// <param name="text">tip text.</param>
    /// <param name="animationType">animation type.</param>
    public Tip(string id, RoleCode roleCode, string text, AnimationType animationType)
        : base(id, text)
    {
        this.TipType = TipType.Job;
        this.RoleCode = roleCode;
        this.AnimationType = animationType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tip"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="id">id of tip.</param>
    /// <param name="roleCode">role id of the tip.</param>
    /// <param name="text">tip text.</param>
    /// <param name="animationCategory">animation category.</param>
    public Tip(string id, RoleCode roleCode, string text, AnimationCategory animationCategory)
        : base(id, text)
    {
        this.TipType = TipType.Job;
        this.RoleCode = roleCode;
        this.AnimationCategory = animationCategory;
    }

    /// <summary>
    /// Gets or sets the type of tip.
    /// </summary>
    public TipType TipType { get; set; }

    /// <summary>
    /// Gets or sets the associated job code.
    /// </summary>
    public JobCode JobCode { get; set; }

    /// <summary>
    /// Gets or sets the associated role code.
    /// </summary>
    public RoleCode RoleCode { get; set; }
}
