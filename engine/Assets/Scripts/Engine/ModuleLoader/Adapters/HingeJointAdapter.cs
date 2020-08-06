using System;
using Engine.Util;
using UnityEngine;
using SynthesisAPI.Utilities;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.Runtime;

using HingeJoint = SynthesisAPI.EnvironmentManager.Components.HingeJoint;
using Rigidbody = SynthesisAPI.EnvironmentManager.Components.Rigidbody;
using ConfigurableJointMotion = SynthesisAPI.EnvironmentManager.Components.ConfigurableJointMotion;
using JointLimits = SynthesisAPI.EnvironmentManager.Components.JointLimits;
using JointMotor = SynthesisAPI.EnvironmentManager.Components.JointMotor;
using System.ComponentModel;

namespace Engine.ModuleLoader.Adapters
{
    // TODO: Add callback for OnJointBreak (for fun)
    public class HingeJointAdapter : MonoBehaviour, IApiAdapter<HingeJoint>
    {
        internal HingeJoint instance;
        internal UnityEngine.HingeJoint unityJoint;

        public void SetInstance(HingeJoint joint)
        {
            instance = joint;
            
            if ((unityJoint = GetComponent<UnityEngine.HingeJoint>()) == null)
                unityJoint = gameObject.AddComponent<UnityEngine.HingeJoint>();

            instance.PropertyChanged += UpdateProperty;

            // Init all variables

            unityJoint.anchor = instance.anchor.Map();
            unityJoint.axis = instance.axis.Map();
            unityJoint.breakForce = instance.breakForce;
            unityJoint.breakTorque = instance.breakTorque;
            unityJoint.connectedBody = ((RigidbodyAdapter)instance.connectedBody?.Adapter).unityRigidbody;
            unityJoint.enableCollision = instance.enableCollision;
            unityJoint.useLimits = instance.useLimits;
            unityJoint.limits = instance.limits.GetUnity();
            unityJoint.useMotor = instance.useMotor;
            unityJoint.motor = instance.motor.GetUnity();
        }

        public void UpdateProperty(object sender, PropertyChangedEventArgs args)
        {
            if (unityJoint == null)
            {
                // TODO: Need to do something else if joint breaks
                SynthesisAPI.Utilities.Logger.Log("Joint is broken, cannot set", LogLevel.Debug);
            }

            switch (args.PropertyName.ToLower())
            {
                case "anchor":
                    unityJoint.anchor = instance.anchor.Map();
                    break;
                case "axis":
                    unityJoint.axis = instance.axis.Map();
                    break;
                case "breakforce":
                    unityJoint.breakForce = instance.breakForce;
                    break;
                case "breakTorque":
                    unityJoint.breakTorque = instance.breakTorque;
                    break;
                case "connectedbody":
                    // TODO
                    unityJoint.connectedBody = ((RigidbodyAdapter)instance.connectedBody.Adapter).unityRigidbody;
                    break;
                case "enablecollision":
                    unityJoint.enableCollision = instance.enableCollision;
                    break;
                case "uselimits":
                    unityJoint.useLimits = instance.useLimits;
                    break;
                case "limits":
                    unityJoint.limits = instance.limits.GetUnity();
                    break;
                case "usemotor":
                    unityJoint.useMotor = instance.useMotor;
                    break;
                case "motor":
                    unityJoint.motor = instance.motor.GetUnity();
                    break;
                default:
                    throw new Exception($"Property {args.PropertyName} not supported");
            }
        }

        public void Update() {
            instance.velocity = unityJoint.velocity;
            instance.angle = unityJoint.angle;
        }

        public static HingeJoint NewInstance() => new HingeJoint();
    }
}
