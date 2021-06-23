#if false
using SynthesisAPI.Modules.Attributes;
using _UnityLayerMask = UnityEngine.LayerMask;

namespace SynthesisAPI.EnvironmentManager.Components
{
    // [BuiltIn]
    public class Layer : Component
    {
        internal int _layerMask;
        internal string _layerName;
        internal bool Changed { get; private set; } = true;
        internal void ProcessedChanges() => Changed = false;

        /*
        public static int GetMask(params string[] layerNames)
        {
            return _UnityLayerMask.GetMask(layerNames);
        }
        */
        public static string LayerToName(int layer)
        {
            return _UnityLayerMask.LayerToName(layer);
        }
        public static int NameToLayer(string layerName)
        {
            return _UnityLayerMask.NameToLayer(layerName);
        }

        public int LayerMask
        {
            get => _layerMask;
            set
            {
                _layerMask = value;
                _layerName = LayerToName(_layerMask);
                Changed = true;
            }
        }

        public string LayerName
        {
            get => _layerName;
            set
            {
                _layerName = value;
                _layerMask = NameToLayer(_layerName);
                Changed = true;
            }
        }
    }
}

#endif