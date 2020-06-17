using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.VirtualFileSystem
{
    public class RawEntry : Entry, IDisposable
    {
        public enum FilePermissions
        {
            Read,
            Write
        }

        public RawEntry(Guid owner, Permissions perm)
        {
            Owner = owner;
            Permissions = perm;
        }

        internal static readonly string BasePath;

        public string Path { get; private set; }

        public FilePermissions FilePerms { get; private set; }

        public override void Delete()
        {
            // TODO
        }

        public void Dispose()
        {
            // TODO
        }

        private byte[]? data;

        private SafeStream Stream;

        private const int DefaultTimeout = 5000;
    }
}
