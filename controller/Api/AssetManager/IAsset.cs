﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynthesisAPI.VirtualFileSystem;

namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// Any type of asset managed by the virtual file system
    /// </summary>
    public interface IAsset : IResource
    {
        /// <summary>
        /// Process asset data to construct a new asset
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IResource Load(byte[] data);
    }

    /// <summary>
    /// 
    /// </summary>
    public static class IAssetExtension // TODO should this really be an extension method?
    {
        public static IResource LoadAsset(this IAsset asset, string path, byte[] data)
        {
            return FileSystem.AddResource(path, asset.Load(data));
        }
    }
}
