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
    }

    public static class IAssetExtension
    {
        public static IResource LoadAsset(this IAsset asset, string path, byte[] data)
        {
            return FileSystem.AddResource(path, asset.Load(data));
        }
    }
}
