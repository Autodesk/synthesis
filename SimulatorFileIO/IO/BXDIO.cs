using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BXDIO
{
    private const byte MAJOR_VERSION = 0;           // Bump on stable releases
    private const byte MINOR_VERSION = 0;           // Bump on beta releases
    private const byte REVISION_VERSION = 9;        // Bump on major IO changes.
    private const byte REVISION_PORTION = 2;        // Bump on IO changes in a meta chunk.

    public const string ASSEMBLY_VERSION = "0.0.9.2";   // I'm so sorry that this isn't dynamic :'(

    public delegate void ProgressReporter(float progress);

    /// <summary>
    /// The version of the BXDJ file format this file can read and write.
    /// </summary>
    public const uint FORMAT_VERSION = (MAJOR_VERSION << 24) | (MINOR_VERSION << 16) | (REVISION_VERSION << 8) | REVISION_PORTION;

    /// <summary>
    /// Converts the given version ID number to decimal notation.
    /// </summary>
    /// <param name="version">Version ID</param>
    /// <returns>Decimal notation of the version ID</returns>
    public static string VersionToString(uint version)
    {
        return ((version >> 24) & 0xFF) + "." + ((version >> 16) & 0xFF) + "." + ((version >> 8) & 0xFF) + "." + ((version >> 0) & 0xFF);
    }

    /// <summary>
    /// Talks about compatibility.
    /// </summary>
    /// <param name="version">The version to compare with</param>
    public static void CheckReadVersion(uint version)
    {
        if ((version & 0xFFFFFF00) == (FORMAT_VERSION & 0xFFFFFF00))
        {
            if (version != FORMAT_VERSION)
            {
                Console.Write("Trying to read version " + VersionToString(version) + " using API version " + VersionToString(FORMAT_VERSION) + ".  Continue? (y/N)  ");
                try
                {
                    String s = Console.ReadLine();
                    if (s.ToLower().Trim().Equals("y"))
                    {
                        Console.WriteLine("Ignoring version mismatch... beware.");
                        return;
                    }
                }
                catch   // User input isn't enabled
                {
                    Console.WriteLine("Ignoring version mismatch... beware.");
                    return;
                }
            }
            else
            {
                return;
            }
        }
        throw new FormatException("Trying to read version " + VersionToString(version) + " using API version " + VersionToString(FORMAT_VERSION));
    }

    // Prevents creation of this class
    private BXDIO()
    {
    }
}