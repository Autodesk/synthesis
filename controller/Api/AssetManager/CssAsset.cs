using SynthesisAPI.VirtualFileSystem;
using System;

namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// Representation of a CSS asset
    /// </summary>
    public class CssAsset : TextAsset
    {
        public CssAsset(string name, Guid owner, Permissions perm, string sourcePath) :
            base(name, owner, perm, sourcePath)
        { }
    }
}
