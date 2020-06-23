using Newtonsoft.Json;
using SynthesisAPI.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// Representation of a plain text asset
    /// </summary>
    public class PlainTextAsset : Asset
    {
        public PlainTextAsset(string name, Guid owner, Permissions perm)
        {
            Init(name, owner, perm);
        }

        public long Seek(long offset, SeekOrigin loc = SeekOrigin.Begin)
        {
            return stream.Seek(offset, loc);
        }

        public StreamReader CreateReader()
        {
            return new StreamReader(stream);
        }

        public void Delete() { }

        public override IResource Load(byte[] data)
        {
            stream = new MemoryStream(data, false);

            return this;
        }

        private MemoryStream stream;
    }
}
