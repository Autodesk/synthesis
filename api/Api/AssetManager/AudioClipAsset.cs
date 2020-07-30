using SynthesisAPI.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SynthesisAPI.AssetManager
{
    public class AudioClipAsset : Asset
    {
        private AudioClip? _clip;

        internal AudioClip? GetClip() => _clip;

        public AudioClipAsset(string name, Permissions perm, string sourcePath)
        {
            Init(name, perm, sourcePath);
        }

        public override IEntry Load(byte[] data)
        {
            string tempFile = Path.GetTempPath() + "\\synthesis_temp.wav";
            File.WriteAllBytes(tempFile, data);
            var request = UnityWebRequestMultimedia.GetAudioClip(tempFile, AudioType.WAV);
            var handle = request.SendWebRequest();
            while (!handle.isDone)
                _ = 0; //
            _clip = DownloadHandlerAudioClip.GetContent(request);
            return this;
        }
    }
}
