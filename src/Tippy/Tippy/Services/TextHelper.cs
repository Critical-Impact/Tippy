using System;
using System.Text;
using Dalamud.Plugin;

namespace Tippy;

/// <summary>
/// Text utility class.
/// </summary>
public class TextHelperService
{
    private readonly IDalamudPluginInterface pluginInterface;

    public TextHelperService(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }
    /// <summary>
    /// Sanitizes text by removing unprintable characters, extra newlines, and reducing length.
    /// </summary>
    /// <param name="text">Text to be sanitized.</param>
    /// <returns>The modified text.</returns>
    public string SanitizeText(string text)
    {
        text = text.Replace($"{Environment.NewLine}", " ");
        text = text.Replace("  ", " ");
        text = this.pluginInterface.Sanitizer.Sanitize(text);
        if (text.Length > 200)
        {
            text = text[..200];
        }

        return text;
    }

    /// <summary>
    /// Word wraps the given text to fit within the specified width.
    /// </summary>
    /// <param name="text">Text to be word wrapped.</param>
    /// <param name="width">Width, in characters, to which the text should be word wrapped.</param>
    /// <returns>The modified text.</returns>
    public string WordWrap(string text, int width)
    {
        // validate inputs
        if (string.IsNullOrEmpty(text) || width < 1) return text;

        // setup
        int pos, next;
        var sb = new StringBuilder();

        // Parse each line of text
        for (pos = 0; pos < text.Length; pos = next)
        {
            // Find end of line
            var eol = text.IndexOf(Environment.NewLine, pos, StringComparison.Ordinal);
            if (eol == -1)
            {
                next = eol = text.Length;
            }
            else
            {
                next = eol + Environment.NewLine.Length;
            }

            // Copy this line of text, breaking into smaller lines as needed
            if (eol > pos)
            {
                do
                {
                    var len = eol - pos;
                    if (len > width)
                    {
                        len = BreakLine(text, pos, width);
                    }

                    sb.Append(text, pos, len);
                    sb.Append(Environment.NewLine);

                    // Trim whitespace following break
                    pos += len;
                    while (pos < eol && char.IsWhiteSpace(text[pos]))
                    {
                        pos++;
                    }
                }
                while (eol > pos);
            }
            else
            {
                sb.Append(Environment.NewLine); // Empty line
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Locates position to break the given line so as to avoid
    /// breaking words.
    /// </summary>
    /// <param name="text">String that contains line of text.</param>
    /// <param name="pos">Index where line of text starts.</param>
    /// <param name="max">Maximum line length.</param>
    /// <returns>The modified line length.</returns>
    private int BreakLine(string text, int pos, int max)
    {
        // Find last whitespace in line
        var i = max;
        while (i >= 0 && !char.IsWhiteSpace(text[pos + i]))
        {
            i--;
        }

        // If no whitespace found, break at maximum length
        if (i < 0)
        {
            return max;
        }

        // Find start of whitespace
        while (i >= 0 && char.IsWhiteSpace(text[pos + i]))
        {
            i--;
        }

        // Return length of text before whitespace
        return i + 1;
    }
}
