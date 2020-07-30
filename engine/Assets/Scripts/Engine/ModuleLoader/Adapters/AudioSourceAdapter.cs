using System;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager.Components;
using UnityEngine;

using AudioSource = SynthesisAPI.EnvironmentManager.Components.AudioSource;

namespace Engine.ModuleLoader.Adapters
{
    class AudioSourceAdapter : MonoBehaviour, IApiAdapter<AudioSource>
    {
        internal AudioSource instance;
        internal UnityEngine.AudioSource unitySource;

        private AudioClipAsset currentClip = null;

        public void SetInstance(AudioSource source)
        {
            instance = source;

            unitySource = gameObject.AddComponent<UnityEngine.AudioSource>();

            instance.LinkedGetter = Getter;
            instance.LinkedSetter = Setter;

            unitySource.playOnAwake = false;
            unitySource.Stop();
            unitySource.time = 0;
        }

        public object Getter(string n)
        {
            switch (n.ToLower())
            {
                case "isplaying":
                    return unitySource.isPlaying;
                case "clip":
                    return currentClip;
                default:
                    throw new Exception();
            }
        }

        public void Setter(string n, object o)
        {
            switch (n.ToLower())
            {
                case "isplaying":
                    if ((bool)o && !unitySource.isPlaying)
                        unitySource.Play();
                    else if (!(bool)o && unitySource.isPlaying)
                        unitySource.Stop();
                    break;
                case "clip":
                    currentClip = (AudioClipAsset)o;
                    unitySource.clip = currentClip.GetClip();
                    break;
                default:
                    throw new Exception();
            }
        }

        public static AudioSource NewInstance() => new AudioSource();
    }
}
