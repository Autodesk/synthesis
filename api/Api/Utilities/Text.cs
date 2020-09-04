using System.Collections.Generic;

namespace SynthesisAPI.Utilities
{
    public static class Text
    {
        /// <summary>
        /// Split a line along new-lines or at the last space before the line exceeds the max line limit
        /// </summary>
        /// <param name="text">The line of text to format</param>
        /// <param name="maxLineLength">Maximum number of characters on a line</param>
        /// <returns></returns>
        public static List<string> SplitLines(string text, int maxLineLength)
        {
            var lines = new List<string>();
            for (var i = 0; i < text.Length;)
            {
                var nextRangeLength = Math.Min(maxLineLength, text.Length - i);
                var newline = text.IndexOf('\n', i, nextRangeLength);
                if (newline != -1) // If there's a new line character, then break there
                {
                    lines.Add(text.Substring(i, newline - i));
                    i = newline + 1;
                }
                else if ((i + nextRangeLength) == text.Length) // If this is the last chunk of text, then break there
                {
                    lines.Add(text.Substring(i, nextRangeLength));
                    i += nextRangeLength;
                }
                else
                {
                    var lastSpace = text.LastIndexOf(' ', i + nextRangeLength - 1, nextRangeLength);
                    if (lastSpace != -1) // Break at last space on the line
                    {
                        lines.Add(text.Substring(i, lastSpace - i));
                        i = lastSpace + 1;
                    }
                    else // Break once the line exceeds maxLineLength
                    {
                        lines.Add(text.Substring(i, nextRangeLength));
                        i += nextRangeLength;
                    }
                }
            }
            return lines;
        }
    }
}
