using System.Text;
using SynthesisAPI.UIManager;
using SynthesisAPI.VirtualFileSystem;

namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// Representation of a CSS asset
    /// </summary>
    public class UssAsset : Asset
    {
        public StyleSheet _styleSheet { get; private set; }
        
        public UssAsset(string name, Permissions perm, string sourcePath)
        {
            Init(name, perm, sourcePath);
        }
        
        public override IEntry Load(byte[] data)
        {
            string[] contents = Encoding.UTF8.GetString(data).Split('\n');
            _styleSheet = new StyleSheet(contents);
            return this;
        }
    }
}
