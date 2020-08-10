using MathNet.Spatial.Euclidean;
using SynthesisAPI.Utilities;
using System.Linq;

namespace Engine.Util
{
    public static class Misc
    {
        public static UnityEngine.GameObject FindGameObject(string name)
        {
            return FindUnityObject<UnityEngine.GameObject>(name);
        }

        public static TGameObject FindUnityObject<TGameObject>(string name) where TGameObject : UnityEngine.Object
        {
            return UnityEngine.Resources.FindObjectsOfTypeAll<TGameObject>().First(o => o.name == name);
        }

    }
}
