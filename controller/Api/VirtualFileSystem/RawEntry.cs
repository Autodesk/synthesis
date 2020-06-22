using System;
using System.IO;
using System.Threading;
using SynthesisAPI.Utilities;

#nullable enable // TODO enable this for the whole project

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

        public string Path { get; private set; }

        public FilePermissions FilePerms { get; private set; }

        public void Load()
        {
            data = File.ReadAllBytes(FileSystem.BasePath + Path);
            RawStream = new MemoryStream();                 // create expandable memory stream
            RawStream.Write(data, 0, data.Length);
            RawStream.Position = 0;

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
