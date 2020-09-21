using UnityEngine;
using System.Collections;
using SynthesisAPI.EnvironmentManager.Components;
using System.Collections.Generic;
using FixedJoint = SynthesisAPI.EnvironmentManager.Components.FixedJoint;
using HingeJoint = SynthesisAPI.EnvironmentManager.Components.HingeJoint;
using System.ComponentModel;
using System;
using SynthesisAPI.Utilities;

using Logger = SynthesisAPI.Utilities.Logger;

namespace Engine.ModuleLoader.Adapters
{
    public class JointsAdapter : MonoBehaviour, IApiAdapter<Joints>
    {
        private Joints instance;
        private List<IJointAdapter> _jointAdapters = new List<IJointAdapter>();

        public void SetInstance(Joints joints)
        {
            instance = joints;
            instance.AddJoint += Add;
            instance.RemoveJoint += Remove;
            
            foreach (var joint in instance.AllJoints)
                Add(joint);
        }

        public static Joints NewInstance => new Joints();

        // Update is called once per frame
        public void Update()
        {
            for (var i = _jointAdapters.Count - 1; i >= 0; i--)
            {
                IJointAdapter jointAdapter = _jointAdapters[i];
                if (!jointAdapter.Exists())
                    _jointAdapters.Remove(jointAdapter);
                else
                    jointAdapter.Update();
            }
        }

        void Add(IJoint joint)
        {
            if (joint is FixedJoint fixedJoint)
            {
                FixedJointAdapter jointAdapter = new FixedJointAdapter(fixedJoint);
                _jointAdapters.Add(jointAdapter);
            }
            else if (joint is HingeJoint hingeJoint)
            {
                HingeJointAdapter jointAdapter = new HingeJointAdapter(hingeJoint);
                _jointAdapters.Add(jointAdapter);
            }
        }

        void Remove(IJoint j)
        {
            _jointAdapters.RemoveAll(x => x.GetIJoint() == j); // Again, might not work
        }

        interface IJointAdapter
        {
            void Update();
            IJoint GetIJoint();
            bool Exists();
        }

        class FixedJointAdapter : IJointAdapter
        {
            private FixedJoint _joint;
            public IJoint GetIJoint() => _joint;
            private UnityEngine.FixedJoint _unityJoint;
            public bool Exists() => _unityJoint != null;
            public FixedJointAdapter(FixedJoint joint)
            {
                _joint = joint;
                _joint.PropertyChanged += UpdateProperty;

                //init
                //TODO: Add parent and child body connections
                _unityJoint = Api.ApiProviderData.GameObjects[_joint.connectedParent.Entity.Value].AddComponent<UnityEngine.FixedJoint>();
                
                _unityJoint.anchor = _joint.anchor.Map() - _unityJoint.transform.position;
                _unityJoint.axis = _joint.axis.Map();
                _unityJoint.breakForce = _joint.breakForce;
                _unityJoint.breakTorque = _joint.breakTorque;
                _unityJoint.connectedBody = ((RigidbodyAdapter)_joint.connectedChild?.Adapter).unityRigidbody;
                _unityJoint.enableCollision = _joint.enableCollision;

                _unityJoint.massScale = _joint.connectedParent.mass / _joint.connectedChild.mass;
            }
            public void Update()
            {

            }
            private void UpdateProperty(object sender, PropertyChangedEventArgs args)
            {
                if (_unityJoint == null)
                {
                    SynthesisAPI.Utilities.Logger.Log("Joint is broken, cannot set", LogLevel.Error);
                    return;
                }

                // TODO: What should we do about the parent?
                switch (args.PropertyName.ToLower())
                {
                    case "axis":
                        _unityJoint.axis = _joint.axis.Map() - _unityJoint.transform.position;
                        break;
                    case "anchor":
                        _unityJoint.anchor = _joint.anchor.Map();
                        break;
                    case "connectedchild":
                        _unityJoint.connectedBody = ((RigidbodyAdapter)_joint.connectedChild.Adapter).unityRigidbody;
                        break;
                    case "breakforce":
                        _unityJoint.breakForce = _joint.breakForce;
                        break;
                    case "breaktorque":
                        _unityJoint.breakTorque = _joint.breakTorque;
                        break;
                    case "enablecollision":
                        _unityJoint.enableCollision = _joint.enableCollision;
                        break;
                    default:
                        throw new Exception($"Property {args.PropertyName} not supported");
                }
            }
        }
        class HingeJointAdapter : IJointAdapter
        {
            private HingeJoint _joint;
            public IJoint GetIJoint() => _joint;
            private UnityEngine.HingeJoint _unityJoint;
            public bool Exists() => _unityJoint != null;
            public HingeJointAdapter(HingeJoint joint)
            {
                _joint = joint;
                _joint.PropertyChanged += UpdateProperty;

                //init all properties
                //TODO: Add parent and child body connections
                _unityJoint = GameObject.Find($"Entity {_joint.connectedParent.Entity?.Index}").AddComponent<UnityEngine.HingeJoint>();

                // Logger.Log($"Creating Joint on \"{_unityJoint.gameObject.name}\""); // Useful for debugging
                
                _unityJoint.anchor = _joint.anchor.Map() - _unityJoint.transform.position;
                _unityJoint.axis = _joint.axis.Map();
                _unityJoint.breakForce = _joint.breakForce;
                _unityJoint.breakTorque = _joint.breakTorque;
                _unityJoint.connectedBody = ((RigidbodyAdapter)_joint.connectedChild?.Adapter).unityRigidbody;
                _unityJoint.enableCollision = _joint.enableCollision;
                _unityJoint.useLimits = _joint.useLimits;
                _unityJoint.limits = _joint.limits.GetUnity();
                _unityJoint.useMotor = _joint.useMotor;
                _unityJoint.motor = _joint.motor.GetUnity();

                _unityJoint.massScale = _joint.connectedParent.mass / _joint.connectedChild.mass;
            }
            public void Update()
            {
                _joint.velocity = _unityJoint.velocity;
                _joint.angle = _unityJoint.angle;
            }
            private void UpdateProperty(object sender, PropertyChangedEventArgs args)
            {
                if (_unityJoint == null)
                {
                    // TODO: Need to do something else if joint breaks
                    SynthesisAPI.Utilities.Logger.Log("Joint is broken, cannot set", LogLevel.Error);
                    return;
                }

                //TODO: Add parent and child body connections
                // TODO: What should we do about the parent?
                switch (args.PropertyName.ToLower())
                {
                    case "anchor":
                        _unityJoint.anchor = _joint.anchor.Map() - _unityJoint.transform.position;
                        break;
                    case "axis":
                        _unityJoint.axis = _joint.axis.Map();
                        break;
                    case "breakforce":
                        _unityJoint.breakForce = _joint.breakForce;
                        break;
                    case "breaktorque":
                        _unityJoint.breakTorque = _joint.breakTorque;
                        break;
                    case "connectedchild":
                        _unityJoint.connectedBody = ((RigidbodyAdapter)_joint.connectedChild.Adapter).unityRigidbody;
                        break;
                    case "enablecollision":
                        _unityJoint.enableCollision = _joint.enableCollision;
                        break;
                    case "uselimits":
                        _unityJoint.useLimits = _joint.useLimits;
                        break;
                    case "limits":
                        _unityJoint.limits = _joint.limits.GetUnity();
                        break;
                    case "usemotor":
                        _unityJoint.useMotor = _joint.useMotor;
                        break;
                    case "motor":
                        _unityJoint.motor = _joint.motor.GetUnity();
                        break;
                    default:
                        throw new Exception($"Property {args.PropertyName} not supported");
                }
            }
        }
    }
    
}
