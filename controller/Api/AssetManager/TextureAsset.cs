using System;
using System.Collections;
using System.IO;
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
            GetTexture(tempFile);
            return this;
        }
        
        private IEnumerator GetTexture(string file)
        {
            var request = UnityWebRequestTexture.GetTexture($"file:////{file}");
            yield return request;
            
            if (request.isNetworkError || request.isHttpError)
                throw new Exception("Failed to load texture");

            _textureData = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }
}