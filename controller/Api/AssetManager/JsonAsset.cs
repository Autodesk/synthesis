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
    /// Representation of a JSON asset
    /// </summary>
    public class JsonAsset : TextAsset
    {
        public JsonAsset(string name, Guid owner, Permissions perm, string sourcePath) :
            base(name, owner, perm, sourcePath) { }

        public TObject Deserialize<TObject>(long offset = long.MaxValue, SeekOrigin loc = SeekOrigin.Begin,
            bool retainPosition = true) // TODO
        {
            long? returnPosition = null;
            if (offset != long.MaxValue)
            {
                if (retainPosition)
                {
                    returnPosition = SharedStream.Stream.Position;
                }
                SharedStream.Seek(offset, loc);
            }

            var obj = JsonConvert.DeserializeObject<TObject>(SharedStream.ReadToEnd());

            if (returnPosition != null)
            {
                SharedStream.Seek(returnPosition.Value);
            }
            return obj;
        }

        public void Serialize<TObject>(TObject obj, WriteMode writeMode = WriteMode.Overwrite)
        {
            if (writeMode == WriteMode.Overwrite)
                SharedStream.Seek(0);
            else
                SharedStream.Seek(0, SeekOrigin.End);
            SharedStream.WriteLine(JsonConvert.SerializeObject(obj, Formatting.Indented));
        }
    }
}
