using System;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SynthesisAPI.AssetManager
{
	public class ProtoAsset : Asset
    {
		public ProtoAsset(string name, Guid owner, Permissions permissions, string path) : 
			base(name, owner, permissions, path) { }
    }
}
