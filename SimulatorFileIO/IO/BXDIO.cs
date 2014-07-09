using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BXDIO
{
    private const byte MAJOR_VERSION = 0;
    private const byte MINOR_VERSION = 0;
    private const byte REVISION_VERSION = 3;
    private const byte REVISION_PORTION = 1;
    /// <summary>
    /// The version of the BXDJ file format this file can read and write.
    /// </summary>
    public const uint FORMAT_VERSION = (MAJOR_VERSION << 24) | (MINOR_VERSION << 16) | (REVISION_VERSION << 8) | REVISION_PORTION;

    // Prevents creation of this class
    private BXDIO()
    {
    }
}