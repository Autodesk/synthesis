using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SynthesisAPI.Utilities;

#nullable enable

namespace SynthesisAPI.VirtualFileSystem
{
    public class RawEntry : Entry, IDisposable
    {
        public enum FilePermissions
        {
            Read,
            Write
        }

        public RawEntry(string name, Guid owner, Permissions perm, string file_path)
        {
            Init(name, owner, perm);

            Path = file_path;
            data = null;
        }

        private const string BasePath = "D:\\synthesis_projects\\synthesis\\";

        public string Path { get; private set; }

        public FilePermissions FilePerms { get; private set; }

        public void Load()
        {
            data = File.ReadAllBytes(BasePath + Path);
            RawStream = new MemoryStream(); // TODO create expandable memory stream
            RawStream.Write(data, 0, data.Length);

            Array.Resize(ref data, 10);

            RWLock = new ReaderWriterLockSlim();
            SharedStream = new SharedBinaryStream<MemoryStream>(RawStream, RWLock, DefaultTimeout);
        }

        public override void Delete()
        {
            // TODO
        }

        public void Dispose()
        {
            // TODO
        }

        private byte[]? data;

        private MemoryStream? RawStream;
        public SharedBinaryStream<MemoryStream>? SharedStream { get; internal set; }

        private ReaderWriterLockSlim? RWLock;

        private const int DefaultTimeout = 5000;
    }
}
