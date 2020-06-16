using System;

namespace Api.VirtualFileSystem
{
    [Flags]
    public enum Permissions
    {
        Private = 0,
        PublicRead = 1,
        PublicWrite = 3,
    }
}
