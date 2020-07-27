using SynthesisAPI.VirtualFileSystem;

namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// Representation of a CSS asset
    /// </summary>
    public class CssAsset : TextAsset
    {
        public CssAsset(string name, Permissions perm, string sourcePath) :
            base(name, perm, sourcePath)
        { }
    }
}
