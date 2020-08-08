using System.Text;
using SynthesisAPI.UIManager;
using SynthesisAPI.VirtualFileSystem;

namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// Representation of a USS asset
    /// </summary>
    public class UssAsset : Asset
    {
        public StyleSheet StyleSheet { get; private set; }
        
        public UssAsset(string name, Permissions perm, string sourcePath)
        {
            Init(name, perm, sourcePath);
        }
        
        public override IEntry Load(byte[] data)
        {
            string[] contents = Encoding.UTF8.GetString(data).Split('\n');
            StyleSheet = new StyleSheet(contents);
            return this;
        }
    }
}
