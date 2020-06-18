using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynthesisAPI.VirtualFileSystem;

namespace SynthesisAPI.AssetManager
{
    public interface IAsset : IResource
    {
        public IResource Load(byte[] data);
        public string Path();
    }

    public static class IAssetExtension
    {
        public static IResource LoadAsset(this IAsset asset, byte[] data)
        {
            return FileSystem.AddResource(asset.Path(), asset.Load(data));
        }
    }
}
