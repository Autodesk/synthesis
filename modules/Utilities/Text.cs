using System.Collections.Generic;

namespace Utilities
{
    public static class Text
    {
        public static List<string> SplitLines(string text, int maxLineLength)
        {
            var lines = new List<string>();
            for (var i = 0; i < text.Length;)
            {
                var nextRangeLength = Math.Min(maxLineLength, text.Length - i);
                var newline = text.IndexOf('\n', i, nextRangeLength);
                if (newline != -1)
                {
                    lines.Add(text.Substring(i, newline - i));
                    i = newline + 1;
                }
                else if ((i + nextRangeLength) == text.Length)
                {
                    lines.Add(text.Substring(i, nextRangeLength));
                    i += nextRangeLength;
                }
                else
                {
                    var j = text.LastIndexOf(' ', i + nextRangeLength - 1, nextRangeLength);
                    if (j != -1)
                    {
                        lines.Add(text.Substring(i, j - i));
                        i = j + 1;
                    }
                    else
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
