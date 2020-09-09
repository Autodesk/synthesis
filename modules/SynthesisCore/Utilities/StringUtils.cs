using System.Text.RegularExpressions;

namespace SynthesisCore.Utilities
{
    public class StringUtils
    {
        /**
         * Converts ThisIsAnExample to This Is An Example using regex
         */
        public static string ReformatCondensedString(string condensedString)
        {
            return Regex.Replace(condensedString, "([a-z])([A-Z])", "$1 $2");
        }
    }
}