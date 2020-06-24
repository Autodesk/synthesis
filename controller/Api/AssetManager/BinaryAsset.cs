using SynthesisAPI.Utilities;
using SynthesisAPI.VirtualFileSystem;
using System;
using System.IO;
using System.Threading;

namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// Representation of a binary asset
    /// </summary>
    public class BinaryTextAsset : Asset
    {
        public BinaryTextAsset(string name, Guid owner, Permissions perm, string source_path)
        {
            Init(name, owner, perm, source_path);
        }

        public void SaveToFile()
        {
            long pos = SharedStream.Stream.Position;
            SharedStream.Seek(0);
            File.WriteAllBytes(SourcePath, SharedStream.ReadToEnd());
            SharedStream.Seek(pos);
        }

        public override IEntry Load(byte[] data)
        {
            var stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            RWLock = new ReaderWriterLockSlim();
            SharedStream = new SharedBinaryStream<MemoryStream>(stream, RWLock);

            return this;
        }

        public SharedBinaryStream<MemoryStream> SharedStream { get; protected set; }
        private ReaderWriterLockSlim RWLock;
    }
}
