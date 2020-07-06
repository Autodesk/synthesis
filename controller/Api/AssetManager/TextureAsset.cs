using System;
using System.Collections;
using System.IO;
using System.Threading;
using SynthesisAPI.VirtualFileSystem;
using UnityEngine;
using UnityEngine.Networking;

namespace SynthesisAPI.AssetManager
{
    public class TextureAsset : Asset
    {
        private Texture _textureData;
        
        public TextureAsset(string name, Guid owner, Permissions perms, string sourcePath)
        {
            Init(name, owner, perms, sourcePath);
        }

        public override IEntry Load(byte[] data)
        {
            string tempFile = Path.GetTempPath() + "\\temp_synthesis_texture.syn";
            File.WriteAllBytes(tempFile, data);
            
            var request = UnityWebRequestTexture.GetTexture($"file:////{file}");
            
            while (request.isDone)
                Thread.Sleep(50);
            
            if (request.isNetworkError || request.isHttpError)
                throw new Exception("Failed to load texture");

            _textureData = ((DownloadHandlerTexture)request.downloadHandler).texture;
            
            return this;
        }
    }
}