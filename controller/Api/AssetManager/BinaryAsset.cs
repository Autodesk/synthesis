using SynthesisAPI.Utilities;
using SynthesisAPI.VirtualFileSystem;
using System;
using System.IO;
using System.Threading;

#nullable enable

namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// Representation of a binary asset
    /// </summary>
    public class BinaryAsset : Asset
    {
        public BinaryAsset(string name, Guid owner, Permissions perm, string sourcePath)
        {
            Init(name, owner, perm, sourcePath);
            RwLock = new ReaderWriterLockSlim();
        }

        [ExposedApi]
        public void SaveToFile()
        {
            using var _ = ApiCallSource.StartExternalCall();
            SaveToFileInner();
        }

        internal void SaveToFileInner()
        {
            ApiCallSource.AssertAccess(Permissions, Access.Write);

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

        [ExposedApi]
        public byte[]? ReadToEnd()
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ReadToEndInner();
        }

        internal byte[]? ReadToEndInner()
        {
            ApiCallSource.AssertAccess(Permissions, Access.Read);
            return SharedStream?.ReadToEnd();
        }

        protected SharedBinaryStream SharedStream { get; set; }
        private ReaderWriterLockSlim RwLock;
    }
}
