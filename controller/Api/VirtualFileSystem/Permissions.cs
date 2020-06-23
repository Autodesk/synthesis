using System;

namespace SynthesisAPI.VirtualFileSystem
{
    /// <summary>
    /// Access permissions for resources in the virtual file system
    /// </summary>
    [Flags]
    public enum Permissions
    {
        Private = 0,                // Accessible only by the Synthesis Core
        PublicRead = 1,             // Also readable by modules
        PublicWrite = 3,            // Also writeable by modules
    }
}
