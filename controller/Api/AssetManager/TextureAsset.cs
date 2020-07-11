using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using SynthesisAPI.Runtime;
using SynthesisAPI.VirtualFileSystem;
using UnityEngine;
using UnityEngine.Networking;
using System.Drawing;

namespace SynthesisAPI.AssetManager
{
    public class TextureAsset : Asset
    {
        private Texture2D _textureData;
        public Texture2D TextureData
        {
            get => _textureData;
        }
        
        public TextureAsset(string name, Permissions perms, string sourcePath)
        {
            Init(name, perms, sourcePath);
        }

        public override IEntry Load(byte[] data)
        {
            _textureData = ApiProvider.CreateUnityType<Texture2D>(1, 1);
            _textureData.LoadImage(data);
            
            return this;
        }
    }
}