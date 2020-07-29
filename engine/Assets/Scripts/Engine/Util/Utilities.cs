using MathNet.Spatial.Euclidean;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Util
{
    public static class Utilities
    {
        public static UnityEngine.GameObject FindGameObject(string name)
        {
            return FindUnityObject<UnityEngine.GameObject>(name);
        }

        public static TGameObject FindUnityObject<TGameObject>(string name) where TGameObject : UnityEngine.Object
        {
            return UnityEngine.Resources.FindObjectsOfTypeAll<TGameObject>().First(o => o.name == name);
        }

        public static UnityEngine.Vector3 MapVector3D(Vector3D vec) =>
            MathUtil.MapVector3D(vec);
        public static Vector3D MapVector3(UnityEngine.Vector3 vec) =>
            MathUtil.MapVector3(vec);
        public static Quaternion MapUnityQuaternion(UnityEngine.Quaternion q) =>
            MathUtil.MapUnityQuaternion(q);
        public static UnityEngine.Quaternion MapQuaternion(Quaternion q) =>
            MathUtil.MapQuaternion(q);
    }
}
