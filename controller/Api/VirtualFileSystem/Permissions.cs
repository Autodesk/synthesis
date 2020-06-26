using System;

namespace SynthesisAPI.VirtualFileSystem
{
    /// <summary>
    /// Access permissions for resources in the virtual file system
    /// </summary>
    [Flags]
    public enum Permissions
    {
        PrivateReadOnly,    // Accessible only by the Synthesis Core
        PrivateReadWrite,   // Writable only by the Synthesis Core
        PublicReadOnly,     // Readable by modules
        PublicReadWrite     // Readable and writeable by modules
    }

    public enum Access
    {
        Read,
        Write
    }

    public static class ApiCallSource
    {
        internal class ExternalCallLifetimeClass : IDisposable
        {
            public void Dispose()
            {
                ApiCallSource.IsInternal = true;
            }
        }
        internal static ExternalCallLifetimeClass ExternalCall()
        {
            IsInternal = false;
            return new ExternalCallLifetimeClass();
        }

        internal static bool IsInternal { get; private set; } = true;

        public static void AssertAccess(Permissions perm, Access access)
        {
            if (CannotAccess(perm, access))
            {
                throw new Exception("Missing permissions");
            }
        }

        public static bool CannotAccess(Permissions perm, Access access)
        {
            return !CanAccess(perm, access);
        }

        public static bool CanAccess(Permissions perm, Access access)
        {
            return perm switch
            {
                Permissions.PrivateReadOnly => IsInternal && access == Access.Read,
                Permissions.PrivateReadWrite => IsInternal,
                Permissions.PublicReadOnly => access == Access.Read || IsInternal,
                Permissions.PublicReadWrite => true,
                _ => false, // Error
            };
        }
    }
}
