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

        private void OnEnable()
        {
            if (instance == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if ((unityJoint = GetComponent<UnityEngine.FixedJoint>()) == null)
                unityJoint = gameObject.AddComponent<UnityEngine.FixedJoint>();

            instance.LinkedSetter = Setter;
            instance.LinkedGetter = Getter;

            // Tod
        }

        private object Getter(string n)
        {
            switch (n.ToLower())
            {
                case "connectedbody":
                    if (unityJoint.connectedBody != null)
                        return unityJoint.connectedBody.gameObject.GetComponent<RigidbodyAdapter>().instance;
                    else
                        return null;
                case "breakforce":
                    return unityJoint.breakForce;
                case "breaktorque":
                    return unityJoint.breakTorque;
                case "enablecollision":
                    return unityJoint.enableCollision;
                default:
                    throw new Exception($"Property {n} not supported");
            }
        }
        private void Setter(string n, object o)
        {
            switch (n.ToLower())
            {
                case "connectedbody":
                    unityJoint.connectedBody = ((RigidbodyAdapter)((Rigidbody)o).Adapter).unityRigidbody;
                    break;
                case "breakforce":
                    unityJoint.breakForce = (float)o;
                    break;
                case "breaktorque":
                    unityJoint.breakTorque = (float)o;
                    break;
                case "enablecollision":
                    unityJoint.enableCollision = (bool)o;
                    break;
                default:
                    throw new Exception($"Property {n} not supported");
            }
        }

        public void SetInstance(FixedJoint joint)
        {
            instance = joint;
            gameObject.SetActive(true);
        }

        public static FixedJoint NewInstance() => new FixedJoint();
    }
}
