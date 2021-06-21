using System;

/// <summary>
/// Utility functions for file I/O
/// </summary>
public class BXDIO
{
    public const byte MAJOR_VERSION = 0;           // Bump on stable releases
    public const byte MINOR_VERSION = 3;           // Bump on beta releases
    public const byte REVISION_VERSION = 3;        // Bump on major IO changes.
    public const byte REVISION_PORTION = 1;        // Bump on IO changes in a meta chunk.

    public const string ASSEMBLY_VERSION = "0.3.3.1";   // I'm so sorry that this isn't dynamic :'(

    public delegate void ProgressReporter(long progress, long total);

    /// <summary>
    /// The version of the file format this file can read and write.
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
        if ((version & 0xFF000000) == (FORMAT_VERSION & 0xFF000000))
        {
            if ((version & 0xFFFF0000) != (FORMAT_VERSION & 0xFFFF0000))
            {
                Console.Write("Trying to read version " + VersionToString(version) + " using API version " + VersionToString(FORMAT_VERSION) + ".  Continue? (y/N)  ");
            }
            return;
        }
        throw new FormatException("Trying to read version " + VersionToString(version) + " using API version " + VersionToString(FORMAT_VERSION));
    }

    // Prevents creation of an instance of this class
    private BXDIO()
    {
    }
}
