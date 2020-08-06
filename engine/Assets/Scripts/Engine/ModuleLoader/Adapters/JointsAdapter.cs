﻿using UnityEngine;
using System.Collections;
using SynthesisAPI.EnvironmentManager.Components;
using System.Collections.Generic;
using FixedJoint = SynthesisAPI.EnvironmentManager.Components.FixedJoint;
using HingeJoint = SynthesisAPI.EnvironmentManager.Components.HingeJoint;
using System.ComponentModel;
using System;
using SynthesisAPI.Utilities;

namespace Engine.ModuleLoader.Adapters
{
    public class JointsAdapter : MonoBehaviour, IApiAdapter<Joints>
    {
        private Joints instance;
        private List<JointAdapter> _jointAdapters = new List<JointAdapter>();

        public void SetInstance(Joints joints)
        {
            instance = joints;
            instance.AddJoint += Add;
            instance.RemoveJoint += Remove;
        }

        public static Joints NewInstance => new Joints();

        // Update is called once per frame
        void Update()
        {
            foreach (JointAdapter jointAdapter in _jointAdapters)
                jointAdapter.Update();
        }

        void Add(IJoint joint)
        {
            if (joint is SynthesisAPI.EnvironmentManager.Components.FixedJoint)
            {
                FixedJointAdapter jointAdapter = new FixedJointAdapter((FixedJoint) joint);
                _jointAdapters.Add(jointAdapter);
            }
            else if (joint is SynthesisAPI.EnvironmentManager.Components.HingeJoint)
            {
                HingeJointAdapter jointAdapter = new HingeJointAdapter((HingeJoint) joint);
                _jointAdapters.Add(jointAdapter);
            }
        }

        void Remove(int jointIndex)
        {
            _jointAdapters.RemoveAt(jointIndex);
        }

        interface JointAdapter
        {
            void Update();
        }

        class FixedJointAdapter : JointAdapter
        {
            private FixedJoint _joint;
            private UnityEngine.FixedJoint _unityJoint;
            public FixedJointAdapter(FixedJoint joint)
            {
                _joint = joint;
                _joint.PropertyChanged += UpdateProperty;

                //init
                //TODO: Add parent and child body connections
                _unityJoint.anchor = _joint.anchor.Map();
                _unityJoint.axis = _joint.axis.Map();
                _unityJoint.breakForce = _joint.breakForce;
                _unityJoint.breakTorque = _joint.breakTorque;
                _unityJoint.connectedBody = ((RigidbodyAdapter)_joint.connectedBody?.Adapter).unityRigidbody;
                _unityJoint.enableCollision = _joint.enableCollision;
            }
            public void Update()
            {

            }
            private void UpdateProperty(object sender, PropertyChangedEventArgs args)
            {
                if (_unityJoint == null)
                {
                    SynthesisAPI.Utilities.Logger.Log("Joint is broken, cannot set", LogLevel.Debug);
                }

                switch (args.PropertyName.ToLower())
                {
                    case "axis":
                        _unityJoint.axis = _joint.axis.Map();
                        break;
                    case "anchor":
                        _unityJoint.anchor = _joint.anchor.Map();
                        break;
                    case "connectedbody":
                        _unityJoint.connectedBody = ((RigidbodyAdapter)_joint.connectedBody.Adapter).unityRigidbody;
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
        class HingeJointAdapter : JointAdapter
        {
            private HingeJoint _joint;
            private UnityEngine.HingeJoint _unityJoint;
            public HingeJointAdapter(HingeJoint joint)
            {
                _joint = joint;
                _joint.PropertyChanged += UpdateProperty;

                //init all properties
                //TODO: Add parent and child body connections
                _unityJoint.anchor = _joint.anchor.Map();
                _unityJoint.axis = _joint.axis.Map();
                _unityJoint.breakForce = _joint.breakForce;
                _unityJoint.breakTorque = _joint.breakTorque;
                _unityJoint.connectedBody = ((RigidbodyAdapter)_joint.connectedBody?.Adapter).unityRigidbody;
                _unityJoint.enableCollision = _joint.enableCollision;
                _unityJoint.useLimits = _joint.useLimits;
                _unityJoint.limits = _joint.limits.GetUnity();
                _unityJoint.useMotor = _joint.useMotor;
                _unityJoint.motor = _joint.motor.GetUnity();
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
                    SynthesisAPI.Utilities.Logger.Log("Joint is broken, cannot set", LogLevel.Debug);
                }

                //TODO: Add parent and child body connections
                switch (args.PropertyName.ToLower())
                {
                    case "anchor":
                        _unityJoint.anchor = _joint.anchor.Map();
                        break;
                    case "axis":
                        _unityJoint.axis = _joint.axis.Map();
                        break;
                    case "breakforce":
                        _unityJoint.breakForce = _joint.breakForce;
                        break;
                    case "breakTorque":
                        _unityJoint.breakTorque = _joint.breakTorque;
                        break;
                    case "connectedbody":
                        _unityJoint.connectedBody = ((RigidbodyAdapter)_joint.connectedBody.Adapter).unityRigidbody;
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
