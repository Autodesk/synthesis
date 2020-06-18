using System;

namespace SynthesisAPI.VirtualFileSystem
{
    [Flags]
    public enum Permissions
    {
        Private = 0,
        PublicRead = 1,
        PublicWrite = 3,
    }
}
