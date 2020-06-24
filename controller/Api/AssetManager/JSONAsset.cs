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
    public class JSONAsset : TextAsset
    {
        public JSONAsset(string name, Guid owner, Permissions perm, string source_path) :
            base(name, owner, perm, source_path) { }

        public TObject Deserialize<TObject>(long offset = long.MaxValue, SeekOrigin loc = SeekOrigin.Begin, bool retainPosition = true) // TODO
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

            TObject obj = JsonConvert.DeserializeObject<TObject>(SharedStream.ReadToEnd());

            if (returnPosition != null)
            {
                SharedStream.Seek(returnPosition.Value);
            }
            return obj;
        }

        public void Serialize<TObject>(TObject obj, WriteMode write_mode = WriteMode.Overwrite)
        {
            if (write_mode == WriteMode.Overwrite)
            {
                SharedStream.Seek(0);
            }

            SharedStream.WriteLine(JsonConvert.SerializeObject(obj, Formatting.Indented));
        }
    }
}
