using System;
using System.Dynamic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.VirtualFileSystem;

namespace SynthesisAPI.AssetManager
{
    public class GltfAsset<T> : Asset
    {

        public GltfAsset(string name, Guid owner, Permissions permissions, string path)
        {
            Init(name, owner, permissions, path);
        }

        public override IEntry Load(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
