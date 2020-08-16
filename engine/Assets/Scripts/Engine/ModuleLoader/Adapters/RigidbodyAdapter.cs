using System;
using Rigidbody = SynthesisAPI.EnvironmentManager.Components.Rigidbody;
using UnityEngine;
using SynthesisAPI.Utilities;
using MathNet.Spatial.Euclidean;
using Engine.Util;

using RigidbodyConstraints = SynthesisAPI.EnvironmentManager.Components.RigidbodyConstraints;
using CollisionDetectionMode = SynthesisAPI.EnvironmentManager.Components.CollisionDetectionMode;
using System.ComponentModel;

namespace Engine.ModuleLoader.Adapters
{
    public class RigidbodyAdapter : MonoBehaviour, IApiAdapter<Rigidbody>
    {
        internal Rigidbody instance;
        internal UnityEngine.Rigidbody unityRigidbody;

        public void SetInstance(Rigidbody rigidbody)
        {
            instance = rigidbody;
            instance.Adapter = this;

            if ((unityRigidbody = GetComponent<UnityEngine.Rigidbody>()) == null)
                unityRigidbody = gameObject.AddComponent<UnityEngine.Rigidbody>();

            instance.PropertyChanged += UpdateProperty;

            unityRigidbody.sleepThreshold = 0;

            unityRigidbody.useGravity = instance.useGravity;
            unityRigidbody.isKinematic = instance.isKinematic;
            unityRigidbody.mass = instance.mass;
            unityRigidbody.velocity = instance.velocity.Map();
            unityRigidbody.drag = instance.drag;
            unityRigidbody.angularVelocity = instance.angularVelocity.Map();
            unityRigidbody.angularDrag = instance.angularDrag;
            unityRigidbody.maxAngularVelocity = instance.maxAngularVelocity;
            unityRigidbody.maxDepenetrationVelocity = instance.maxDepenetrationVelocity;
            unityRigidbody.collisionDetectionMode = instance.collisionDetectionMode.Convert<UnityEngine.CollisionDetectionMode>();
            unityRigidbody.constraints = instance.constraints.Convert<UnityEngine.RigidbodyConstraints>();
        }

        private void OnCollisionEnter(Collision collision) => instance.OnEnterCollision(collision.impulse.magnitude);

        private void UpdateProperty(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName.ToLower())
            {
                case "usegravity":
                    unityRigidbody.useGravity = instance.useGravity;
                    break;
                case "iskinematic":
                    unityRigidbody.isKinematic = instance.isKinematic;
                    break;
                case "issleeping":
                    if (instance.isSleeping && !unityRigidbody.IsSleeping())
                        unityRigidbody.Sleep();
                    else if (!instance.isSleeping && unityRigidbody.IsSleeping())
                        unityRigidbody.WakeUp();
                    break;
                case "mass":
                    unityRigidbody.mass = instance.mass;
                    break;
                case "velocity":
                    unityRigidbody.velocity = instance.velocity.Map();
                    break;
                case "drag":
                    unityRigidbody.drag = instance.drag;
                    break;
                case "angularvelocity":
                    unityRigidbody.angularVelocity = instance.angularVelocity.Map();
                    break;
                case "angulardrag":
                    unityRigidbody.angularDrag = instance.angularDrag;
                    break;
                case "maxangularvelocity":
                    unityRigidbody.maxAngularVelocity = instance.maxAngularVelocity;
                    break;
                case "maxdepenetrationvelocity":
                    unityRigidbody.maxDepenetrationVelocity = instance.maxDepenetrationVelocity;
                    break;
                case "collisiondetectionmode":
                    unityRigidbody.collisionDetectionMode = ConvertEnum<UnityEngine.CollisionDetectionMode>(instance.collisionDetectionMode);
                    break;
                case "constraints":
                    unityRigidbody.constraints = ConvertEnum<UnityEngine.RigidbodyConstraints>(instance.constraints);
                    break;
                default:
                    throw new Exception($"Property {args.PropertyName} is not setup");
            }
        }

        public void Update()
        {
            //instance.useGravity = unityRigidbody.useGravity;
            //instance.isKinematic = unityRigidbody.isKinematic;
            instance.mass = unityRigidbody.mass;
            instance.velocity = unityRigidbody.velocity.Map();
            //instance.drag = unityRigidbody.drag;
            instance.angularVelocity = unityRigidbody.angularVelocity.Map();
            //instance.angularDrag = unityRigidbody.angularDrag;
            //instance.maxAngularVelocity = unityRigidbody.maxAngularVelocity;
            //instance.maxDepenetrationVelocity = unityRigidbody.maxDepenetrationVelocity;
            //instance.collisionDetectionMode = unityRigidbody.collisionDetectionMode.Convert<CollisionDetectionMode>();
            //instance.constraints = unityRigidbody.constraints.Convert<RigidbodyConstraints>();

            if (instance.AdditionalForces.Count > 0)
            {
                foreach (var force in instance.AdditionalForces)
                {
                    if (force.Position != Vector3D.NaN)
                        unityRigidbody.AddForceAtPosition(force.Force.Map(), force.Position.Map(), ConvertEnum<ForceMode>(force.Mode));
                    else
                        if (force.Relative)
                        unityRigidbody.AddRelativeForce(force.Force.Map(), ConvertEnum<ForceMode>(force.Mode));
                    else
                        unityRigidbody.AddForce(force.Force.Map(), ConvertEnum<ForceMode>(force.Mode));
                }
                instance.AdditionalForces.Clear();
            }

            if (instance.AdditionalTorques.Count > 0)
            {
                foreach (var torque in instance.AdditionalTorques)
                {
                    if (torque.Relative)
                        unityRigidbody.AddRelativeTorque(torque.Torque.Map(), ConvertEnum<ForceMode>(torque.Mode));
                    else
                        unityRigidbody.AddTorque(torque.Torque.Map(), ConvertEnum<ForceMode>(torque.Mode));
                }
                instance.AdditionalTorques.Clear();
            }
        }

        public static Rigidbody NewInstance()
        {
            return new Rigidbody();
        }

        public TResult ConvertEnum<TResult>(object i) => (TResult)Enum.Parse(typeof(TResult), i.ToString(), true);
    }
}
