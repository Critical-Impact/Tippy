using System;

namespace Tippy;

/// <summary>
/// Tippy Message.
/// </summary>
public class Message
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Message"/> class.
    /// Create message.
    /// </summary>
    /// <param name="text">message text.</param>
    /// <param name="animationType">animation type.</param>
    public Message(string text, AnimationType animationType)
    {
        this.Id = Guid.NewGuid().ToString();
        this.AnimationType = animationType;
        this.Text = text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Message"/> class.
    /// Create message.
    /// </summary>
    /// <param name="text">message text.</param>
    /// <param name="animationCategory">animation category.</param>
    public Message(string text, AnimationCategory animationCategory)
    {
        this.Id = Guid.NewGuid().ToString();
        this.AnimationCategory = animationCategory;
        this.Text = text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Message"/> class.
    /// Create message.
    /// </summary>
    /// <param name="id">id of message.</param>
    /// <param name="text">message text.</param>
    public Message(string id, string text)
    {
        this.Id = id;
        this.Text = text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Message"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="id">id of message.</param>
    /// <param name="text">message text.</param>
    /// <param name="animationType">animation type.</param>
    public Message(string id, string text, AnimationType animationType)
    {
        this.Id = id;
        this.AnimationType = animationType;
        this.Text = text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Message"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="id">id of message.</param>
    /// <param name="text">message text.</param>
    /// <param name="animationCategory">animation category.</param>
    public Message(string id, string text, AnimationCategory animationCategory)
    {
        this.Id = id;
        this.AnimationCategory = animationCategory;
        this.Text = text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Message"/> class.
    /// Create tip.
    /// </summary>
    /// <param name="text">tip text.</param>
    public Message(string text)
    {
        this.Id = Guid.NewGuid().ToString();
        this.Text = text;
    }

    /// <summary>
    /// Gets id of message.
    /// </summary>
    public string Id { get; protected init; }

    /// <summary>
    /// Gets or sets text of message.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets animation type to be used for the message.
    /// </summary>
    public AnimationType? AnimationType { get; protected init; }

    /// <summary>
    /// Gets animation category pool to be used for the message.
    /// </summary>
    public AnimationCategory? AnimationCategory { get; protected init; }

    /// <summary>
    /// Gets a value indicating whether gets or sets indicator to loop animation.
    /// </summary>
    public bool LoopAnimation { get; init; }

    /// <summary>
    /// Gets or sets the source of message.
    /// </summary>
    public MessageSource Source { get; set; } = MessageSource.Default;
}
