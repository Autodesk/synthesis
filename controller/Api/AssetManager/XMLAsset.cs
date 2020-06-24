using SynthesisAPI.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// Representation of an XML asset
    /// </summary>
    public class XMLAsset : TextAsset
    {
        public XMLAsset(string name, Guid owner, Permissions perm, string source_path) :
            base(name, owner, perm, source_path) { }

        public TObject Deserialize<TObject>(long offset = long.MaxValue, SeekOrigin loc = SeekOrigin.Begin, bool retainPosition = true)
        {
            long? returnPosition = null;
            if (offset != long.MaxValue)
            {
                if(retainPosition)
                {
                    returnPosition = SharedStream.Stream.Position;
                }
                SharedStream.Seek(offset, loc);
            }

            TObject obj = (TObject)new XmlSerializer(typeof(TObject)).Deserialize(new StreamReader(SharedStream.Stream));

            if (returnPosition != null)
            {
                SharedStream.Seek(returnPosition.Value);
            }
            return obj;
        }
    }
}
