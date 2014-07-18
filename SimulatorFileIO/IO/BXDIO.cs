using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BXDIO
{
    private const byte MAJOR_VERSION = 0;           // Bump on stable releases
    private const byte MINOR_VERSION = 0;           // Bump on beta releases
    private const byte REVISION_VERSION = 6;        // Bump on major IO changes.
    private const byte REVISION_PORTION = 3;        // Bump on minor additions and removals from IO.

    public const string ASSEMBLY_VERSION = "0.0.6.3";   // I'm so sorry that this isn't dynamic :'(

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
        return ((version >> 24) & 0xFF) + "." + ((version >> 16) & 0xFF) + "." + ((version >> 8) & 0xFF) + ((version >> 0) & 0xFF);
    }

    // Prevents creation of this class
    private BXDIO()
    {
    }
}