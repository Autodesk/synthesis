using SynthesisAPI.Utilities;
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

        [ExposedApi]
        public TObject Deserialize<TObject>(long offset = long.MaxValue, SeekOrigin loc = SeekOrigin.Begin, bool retainPosition = true)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return DeserializeImpl<TObject>(offset, loc, retainPosition);
        }

        internal TObject DeserializeImpl<TObject>(long offset = long.MaxValue, SeekOrigin loc = SeekOrigin.Begin, bool retainPosition = true)
        {
            ApiCallSource.AssertAccess(Permissions, Access.Read);
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

        [ExposedApi]
        public void Serialize<TObject>(TObject obj, WriteMode writeMode = WriteMode.Overwrite)
        {
            using var _ = ApiCallSource.StartExternalCall();
            SerializeImpl<TObject>(obj, writeMode);
        }

        internal void SerializeImpl<TObject>(TObject obj, WriteMode writeMode = WriteMode.Overwrite)
        {
            ApiCallSource.AssertAccess(Permissions, Access.Write);
            if (writeMode == WriteMode.Overwrite)
            {
                SharedStream.Seek(0);
                SharedStream.SetLength(0);
            }
            else
            {
                SharedStream.Seek(0, SeekOrigin.End);
            }

            using (var writer = new StringWriter())
            {
                new XmlSerializer(typeof(TObject)).Serialize(writer, obj);
                SharedStream.WriteLine(writer.ToString());
            }
        }
    }
}
