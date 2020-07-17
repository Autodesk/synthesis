using MathNet.Spatial.Euclidean;
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
            new UnityEngine.Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
        public static Vector3D MapVector3(UnityEngine.Vector3 vec) =>
            new Vector3D(vec.x, vec.y, vec.z);
        public static Quaternion MapUnityQuaternion(UnityEngine.Quaternion q) =>
            new Quaternion(q.w, q.x, q.y, q.z);
        public static UnityEngine.Quaternion MapQuaternion(Quaternion q) =>
            new UnityEngine.Quaternion((float)q.ImagX, (float)q.ImagY, (float)q.ImagZ, (float)q.Real);
    }
}
