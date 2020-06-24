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
    public class XmlAsset : TextAsset
    {
        public XmlAsset(string name, Guid owner, Permissions perm, string sourcePath) :
            base(name, owner, perm, sourcePath) { }

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

        public void Serialize<TObject>(TObject obj, WriteMode writeMode = WriteMode.Overwrite)
        {
            if (writeMode == WriteMode.Overwrite)
                SharedStream.Seek(0);
            else
                SharedStream.Seek(0, SeekOrigin.End);

            using (var writer = new StringWriter())
            {
                new XmlSerializer(typeof(TObject)).Serialize(writer, obj);
                SharedStream.WriteLine(writer.ToString());
            }
        }
    }
}
