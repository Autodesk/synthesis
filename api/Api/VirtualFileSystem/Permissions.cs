using System;

namespace SynthesisAPI.VirtualFileSystem
{
    /// <summary>
    /// Access permissions for entries in the virtual file system
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

    public class PermissionsExpcetion : Exception
    {
        public PermissionsExpcetion() { }

        public PermissionsExpcetion(string message) : base(message) { }

        public PermissionsExpcetion(string message, Exception inner) : base(message, inner) { }
    }

    public static class ApiCallSource
    {
        internal class ExternalCallLifetimeClass : IDisposable
        {
            public void Dispose()
            {
                ApiCallSource._externalCalls -= 1;
            }
        }

        internal class InternalCallLifetimeClass : IDisposable
        {
            public void Dispose()
            {
                ApiCallSource._forceInternal -= 1;
            }
        }

        internal static ExternalCallLifetimeClass StartExternalCall()
        {
            _externalCalls += 1;
            return new ExternalCallLifetimeClass();
        }

        internal static InternalCallLifetimeClass ForceInternalCall()
        {
            _forceInternal += 1;
            return new InternalCallLifetimeClass();
        }

        private static uint _forceInternal;

        private static uint _externalCalls;

        internal static bool IsInternal => _externalCalls == 0 || _forceInternal > 0;

        public static void AssertAccess(Permissions perm, Access access)
        {
            if (CannotAccess(perm, access))
            {
                throw new PermissionsExpcetion("Missing required permissions: Permissions: " + perm.ToString() +
                                               ", Operation: " + (IsInternal ? "Private" : "Public") + " " +
                                               access.ToString());
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
