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
        public BinaryTextAsset(string name, Guid owner, Permissions perm, string sourcePath)
        {
            Init(name, owner, perm, sourcePath);
            RwLock = new ReaderWriterLockSlim();
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
            SharedStream = new SharedBinaryStream(stream, RwLock);

            return this;
        }

        protected SharedBinaryStream SharedStream { get; set; }
        private ReaderWriterLockSlim RwLock;
    }
}
