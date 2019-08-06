namespace InventorRobotExporter.Utilities
{
    static internal class StringUtils
    {
        public static string CapitalizeFirstLetter(string str, bool onlyFirst = false)
        {
            if (str.Length < 2)
                return str.ToUpperInvariant();
            if (onlyFirst)
                return str.Substring(0, 1).ToUpperInvariant() + str.Substring(1).ToLowerInvariant();
            return str.Substring(0, 1).ToUpperInvariant() + str.Substring(1);
        }
    }
}