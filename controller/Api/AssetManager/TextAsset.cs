using Newtonsoft.Json;
using SynthesisAPI.Utilities;
using SynthesisAPI.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public TextAsset(string name, Guid owner, Permissions perm, string source_path)
        {
            Init(name, owner, perm, source_path);
        }

        public void SaveToFile()
        {
            long pos = SharedStream.Stream.Position;
            SharedStream.Seek(0);
            File.WriteAllText(SourcePath, SharedStream.ReadToEnd());
            SharedStream.Seek(pos);
        }

        public override IEntry Load(byte[] data)
        {
            var stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            RWLock = new ReaderWriterLockSlim();
            SharedStream = new SharedTextStream<MemoryStream>(stream, RWLock);

            return this;
        }

        public SharedTextStream<MemoryStream> SharedStream { get; protected set; }
        private ReaderWriterLockSlim RWLock;
    }
}
