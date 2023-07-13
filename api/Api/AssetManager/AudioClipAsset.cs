using SynthesisAPI.VirtualFileSystem;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace SynthesisAPI.AssetManager
{
    public class AudioClipAsset : Asset
    {
        private AudioClip _clip;

        // this should always return a clip but it's up to the user to decide if it's valid
        internal AudioClip GetClip() => _clip;

        public AudioClipAsset(string name, Permissions perm, string sourcePath)
        {
            Init(name, perm, sourcePath);

            // create empty clip
            _clip = AudioClip.Create("EmptyClip", 44100, 1, 44100, false);
            float[] data = new float[44100];
            _clip.SetData(data, 0);
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
