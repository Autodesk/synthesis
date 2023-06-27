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
        private readonly Stream _stream;
        private SharedBinaryStream SharedStream { get; set; } = null!;
        private readonly ReaderWriterLockSlim _rwLock;

        public BinaryAsset(string name, Permissions perm, string sourcePath)
        {
            Init(name, perm, sourcePath);
            _rwLock = new ReaderWriterLockSlim();
            _stream = new MemoryStream();
        }

        [ExposedApi]
        public void SaveToFile()
        {
            using var _ = ApiCallSource.StartExternalCall();
            SaveToFileInner();
        }

        private void SaveToFileInner()
        {
            ApiCallSource.AssertAccess(Permissions, Access.Write);

            long pos = SharedStream.Stream.Position;
            SharedStream.Seek(0);
            File.WriteAllBytes(SourcePath, SharedStream.ReadToEnd());
            SharedStream.Seek(pos);
        }

        public override IEntry Load(byte[] data)
        {
            _stream.Write(data, 0, data.Length);
            _stream.Position = 0;
            SharedStream = new SharedBinaryStream(_stream, _rwLock);

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

        internal override void DeleteInner()
        {
            base.DeleteInner();
            _stream.Close();
        }
    }
}
