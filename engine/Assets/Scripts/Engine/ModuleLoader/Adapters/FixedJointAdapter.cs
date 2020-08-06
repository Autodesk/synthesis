using System.ComponentModel;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using System;
using UnityEngine;

using FixedJoint = SynthesisAPI.EnvironmentManager.Components.FixedJoint;
using Rigidbody = SynthesisAPI.EnvironmentManager.Components.Rigidbody;

namespace Engine.ModuleLoader.Adapters
{
    public class FixedJointAdapter : MonoBehaviour, IApiAdapter<FixedJoint>
    {
        internal UnityEngine.FixedJoint unityJoint;
        internal FixedJoint instance;

        public void SetInstance(FixedJoint joint)
        {
            instance = joint;
            if ((unityJoint = GetComponent<UnityEngine.FixedJoint>()) == null)
                unityJoint = gameObject.AddComponent<UnityEngine.FixedJoint>();

            instance.PropertyChanged += UpdateProperty;

            unityJoint.axis = instance.axis.Map();
            unityJoint.anchor = instance.anchor.Map();
        }

        private void UpdateProperty(object sender, PropertyChangedEventArgs args)
        {
            if (unityJoint == null)
            {
                SynthesisAPI.Utilities.Logger.Log("Joint is broken, cannot set", LogLevel.Debug);
            }

            switch (args.PropertyName.ToLower())
            {
                case "axis":
                    unityJoint.axis = instance.axis.Map();
                    break;
                case "anchor":
                    unityJoint.anchor = instance.anchor.Map();
                    break;
                case "connectedbody":
                    unityJoint.connectedBody = ((RigidbodyAdapter)instance.connectedBody.Adapter).unityRigidbody;
                    break;
                case "breakforce":
                    unityJoint.breakForce = instance.breakForce;
                    break;
                case "breaktorque":
                    unityJoint.breakTorque = instance.breakTorque;
                    break;
                case "enablecollision":
                    unityJoint.enableCollision = instance.enableCollision;
                    break;
                default:
                    throw new Exception($"Property {args.PropertyName} not supported");
            }
        }

        public static FixedJoint NewInstance() => new FixedJoint();
    }
}
