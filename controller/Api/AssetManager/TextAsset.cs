using SynthesisAPI.Utilities;
using SynthesisAPI.VirtualFileSystem;
using System.IO;
using System.Threading;

#nullable enable

namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// Representation of a plain text asset
    /// </summary>
    public class TextAsset : Asset
    {
        public enum WriteMode
        {
            Append,
            Overwrite
        }

        public TextAsset(string name, Permissions perm, string sourcePath)
        {
            Init(name, perm, sourcePath);

            _rwLock = new ReaderWriterLockSlim();
            SharedStream = new SharedTextStream(new MemoryStream(), _rwLock);
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

            var pos = SharedStream.Stream.Position;
            SharedStream.Seek(0);
            File.WriteAllText(SourcePath, SharedStream.ReadToEnd());
            SharedStream.Seek(pos);
        }

        public override IEntry Load(byte[] data)
        {
            var stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            SharedStream = new SharedTextStream(stream, _rwLock)!;

            return this;
        }

        [ExposedApi]
        public string? ReadToEnd()
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ReadToEndInner();
        }

        internal string? ReadToEndInner()
        {
            ApiCallSource.AssertAccess(Permissions, Access.Read);
            return SharedStream?.ReadToEnd();
        }

        protected SharedTextStream SharedStream { get; set; }
        private readonly ReaderWriterLockSlim _rwLock;
    }
}
