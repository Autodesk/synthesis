using System;
using System.Dynamic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.VirtualFileSystem;
using glTFLoader;
using glTFLoader.Schema;

namespace SynthesisAPI.AssetManager
{
    public class GltfAsset : Asset
    {
        private string PathToDir;

        //public GltfAsset(string name, Guid owner, Permissions permissions, string path)
        //{
        //    Init(name, owner, permissions, path);
        //}

        public void Init()
        {
            PathToDir = @"\Users\t_corbk\Documents\GitHub\synthesis\controller\Api\AssetManager\Full_Robot_Rough_v10_1593496385.glb";
            GetGLTF(PathToDir);
        }

        private static glTFLoader.Schema.Gltf GetGLTF(string filePath)
        {
            var deserializedFile = Interface.LoadModel(filePath);

            return deserializedFile;
        }

        public override IEntry Load(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
