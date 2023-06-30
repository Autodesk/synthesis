using Newtonsoft.Json;
using SynthesisAPI.Utilities;
using SynthesisAPI.VirtualFileSystem;
using System.IO;

namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// Representation of a JSON asset
    /// </summary>
    public class JsonAsset : TextAsset
    {
        public JsonAsset(string name, Permissions perm, string sourcePath) :
            base(name, perm, sourcePath) { }

        [ExposedApi]
        public TObject Deserialize<TObject>(long offset = long.MaxValue, SeekOrigin loc = SeekOrigin.Begin,
            bool retainPosition = true)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return DeserializeInner<TObject>(offset, loc, retainPosition);
        }


        internal TObject DeserializeInner<TObject>(long offset = long.MaxValue, SeekOrigin loc = SeekOrigin.Begin,
            bool retainPosition = true)
        {
            ApiCallSource.AssertAccess(Permissions, Access.Read);
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

            if (obj == null) {
                throw new System.Exception("Failed to deserialize a Json object!");
            }

            return obj;
        }

        [ExposedApi]
        public void Serialize<TObject>(TObject obj, WriteMode writeMode = WriteMode.Overwrite)
        {
            using var _ = ApiCallSource.StartExternalCall();
            SerializeInner(obj, writeMode);
        }

        internal void SerializeInner<TObject>(TObject obj, WriteMode writeMode = WriteMode.Overwrite)
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
            SharedStream.WriteLine(JsonConvert.SerializeObject(obj, Formatting.Indented));
        }
    }
}
