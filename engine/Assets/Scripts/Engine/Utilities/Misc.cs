using MathNet.Spatial.Euclidean;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

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


        public static UnityEngine.Vector3 MapVector3D(Vector3D vec) =>
            MathUtil.MapVector3D(vec);
        public static Vector3D MapVector3(UnityEngine.Vector3 vec) =>
            MathUtil.MapVector3(vec);
        public static Quaternion MapUnityQuaternion(UnityEngine.Quaternion q) =>
            MathUtil.MapUnityQuaternion(q);
        public static UnityEngine.Quaternion MapQuaternion(Quaternion q) =>
            MathUtil.MapQuaternion(q);

        public static IEnumerable<TOutput> MapAll<TInput, TOutput>(IEnumerable<TInput> input, Func<TInput, TOutput> converter)
        {
            if (input == null)
                return null;

            List<TOutput> output = new List<TOutput>();

            foreach(var i in input)
            {
                output.Add(converter(i));
            }

            return output;
        }

        public static TOutput[] MapAllToArray<TInput, TOutput>(IEnumerable<TInput> input, Func<TInput, TOutput> converter)
        {
            if (input == null)
                return null;

            List<TOutput> output = new List<TOutput>();

            foreach (var i in input)
            {
                output.Add(converter(i));
            }

            return output.ToArray();
        }

        public static EventTrigger.Entry MakeEventTriggerEntry(EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener(action);
            return entry;
        }
    }
}
