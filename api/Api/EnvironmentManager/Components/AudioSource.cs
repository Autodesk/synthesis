using SynthesisAPI.AssetManager;
using SynthesisAPI.Modules.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI.EnvironmentManager.Components
{
    [BuiltinComponent]
    public class AudioSource : Component
    {
        internal Action<string, object> LinkedSetter;
        internal Func<string, object> LinkedGetter;

        private void Set(string n, object o) => LinkedSetter(n, o);
        private T Get<T>(string n) => (T)LinkedGetter(n);

        public bool IsPlaying {
            get => Get<bool>("isplaying");
            set => Set("isplaying", value);
        }
        public AudioClipAsset AudioClip {
            set => Set("clip", value);
            get => Get<AudioClipAsset>("clip");
        }
    }
}
