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

namespace Engine.ModuleLoader.Adapters
{
    // TODO: Add callback for OnJointBreak (for fun)
    public class HingeJointAdapter : MonoBehaviour, IApiAdapter<HingeJoint>
    {
        internal HingeJoint instance;
        internal UnityEngine.HingeJoint unityJoint;

        private void OnEnable()
        {
            if (instance == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if ((unityJoint = GetComponent<UnityEngine.HingeJoint>()) == null)
                unityJoint = gameObject.AddComponent<UnityEngine.HingeJoint>();

            instance.LinkedGetter = Getter;
            instance.LinkedSetter = Setter;

            // unityJoint.lim

            // TOOD
        }

        public object Getter(string n)
        {
            if (unityJoint == null)
            {
                ApiProvider.Log("Joint is broken, cannot get", LogLevel.Debug);
            }

            switch (n.ToLower())
            {
                case "anchor":
                    return unityJoint.anchor.Map();
                case "axis":
                    return unityJoint.axis.Map();
                case "breakforce":
                    return unityJoint.breakForce;
                case "breaktorque":
                    return unityJoint.breakTorque;
                case "connectedbody":
                    if (unityJoint.connectedBody == null)
                        return null;
                    else
                        return unityJoint.connectedBody.gameObject.GetComponent<RigidbodyAdapter>().instance; // TODO
                case "enablecollision":
                    return unityJoint.enableCollision;
                case "uselimits":
                    return unityJoint.useLimits;
                // case ""
                case "limits":
                    return new JointLimits(unityJoint.limits);
                case "velocity":
                    return unityJoint.velocity;
                case "angle":
                    return unityJoint.angle;
                case "usemotor":
                    return unityJoint.useMotor;
                case "motor":
                    return new JointMotor(unityJoint.motor);
                default:
                    throw new Exception($"Property {n} not supported");
            }
        }

        public void Setter(string n, object o)
        {
            if (unityJoint == null)
            {
                ApiProvider.Log("Joint is broken, cannot set", LogLevel.Debug);
            }

            switch (n.ToLower())
            {
                case "anchor":
                    unityJoint.anchor = ((Vector3D)o).Map();
                    break;
                case "axis":
                    unityJoint.axis = ((Vector3D)o).Map();
                    break;
                case "breakforce":
                    unityJoint.breakForce = (float)o;
                    break;
                case "breakTorque":
                    unityJoint.breakTorque = (float)o;
                    break;
                case "connectedbody":
                    // TODO
                    unityJoint.connectedBody = ((RigidbodyAdapter)((Rigidbody)o).Adapter).unityRigidbody;
                    break;
                case "enablecollision":
                    unityJoint.enableCollision = (bool)o;
                    break;
                case "uselimits":
                    unityJoint.useLimits = (bool)o;
                    break;
                case "limits":
                    unityJoint.limits = ((JointLimits)o).GetUnity();
                    break;
                case "usemotor":
                    unityJoint.useMotor = (bool)o;
                    break;
                case "motor":
                    unityJoint.motor = ((JointMotor)o).GetUnity();
                    break;
                default:
                    throw new Exception($"Property {n} not supported");
            }
        }

        public void SetInstance(HingeJoint joint)
        {
            instance = joint;
            gameObject.SetActive(true);
        }

        public static HingeJoint NewInstance() => new HingeJoint();
    }
}
