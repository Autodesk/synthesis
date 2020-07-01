using System;
using System.Dynamic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SynthesisAPI.AssetManager
{
	// path is the source to the protobuf file
	// ProtoAsset reads the data
	public class ProtoAsset : Asset
    {
		public ProtoAsset(string name, Guid owner, Permissions permissions, string path) : 
			base(name, owner, permissions, path) { }

		public GetProtoFile(string path)
        {
			FileStream fs = new FileStream(path, FileMode.Open);
			ProtoItem protoObject = ProtoItem.Parser.ParseFrom(fs);
			fs.Close();
			return protoObject;
        }
    }
}
