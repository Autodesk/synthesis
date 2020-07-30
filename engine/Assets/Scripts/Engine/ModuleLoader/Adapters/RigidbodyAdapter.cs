using System;
using Rigidbody = SynthesisAPI.EnvironmentManager.Components.Rigidbody;
using UnityEngine;
using SynthesisAPI.Utilities;
using MathNet.Spatial.Euclidean;
using Engine.Util;

using RigidbodyConstraints = SynthesisAPI.EnvironmentManager.Components.RigidbodyConstraints;
using CollisionDetectionMode = SynthesisAPI.EnvironmentManager.Components.CollisionDetectionMode;

namespace Engine.ModuleLoader.Adapters
{
    public class RigidbodyAdapter : MonoBehaviour, IApiAdapter<Rigidbody>
    {
        internal Rigidbody instance;
        internal UnityEngine.Rigidbody unityRigidbody;

        public void OnEnable()
        {
            if (instance == null)
            {
                gameObject.SetActive(false);
                return;
            }

            instance.Adapter = this;

            if ((unityRigidbody = GetComponent<UnityEngine.Rigidbody>()) == null)
                unityRigidbody = gameObject.AddComponent<UnityEngine.Rigidbody>();

            // Setup linked getter and setter;
            instance.LinkedGetter = Getter;
            instance.LinkedSetter = Setter;
        }

        private void OnCollisionEnter(Collision collision) => instance.OnEnterCollision(collision.impulse.magnitude);

        private object Getter(string n)
        {
            switch (n.ToLower())
            {
                case "usegravity":
                    return unityRigidbody.useGravity;
                case "iskinematic":
                    return unityRigidbody.isKinematic;
                case "mass":
                    return unityRigidbody.mass;
                case "velocity":
                    return unityRigidbody.velocity.Map();
                case "drag":
                    return unityRigidbody.drag;
                case "angularvelocity":
                    return unityRigidbody.angularVelocity.Map();
                case "angulardrag":
                    return unityRigidbody.angularDrag;
                case "maxangularvelocity":
                    return unityRigidbody.maxAngularVelocity;
                case "maxdepenetrationvelocity":
                    return unityRigidbody.maxDepenetrationVelocity;
                case "collisiondetectionmode":
                    return ConvertEnum<CollisionDetectionMode>(unityRigidbody.collisionDetectionMode);
                case "constraints":
                    return ConvertEnum<RigidbodyConstraints>(unityRigidbody.constraints);
                default:
                    throw new Exception($"Property {n} is not setup");
            }
        }

        private void Setter(string n, object o)
        {
            switch (n.ToLower())
            {
                case "usegravity":
                    unityRigidbody.useGravity = (bool)o;
                    break;
                case "iskinematic":
                    unityRigidbody.isKinematic = (bool)o;
                    break;
                case "mass":
                    unityRigidbody.mass = (float)o;
                    break;
                case "velocity":
                    unityRigidbody.velocity = ((Vector3D)o).Map();
                    break;
                case "drag":
                    unityRigidbody.drag = (float)o;
                    break;
                case "angularvelocity":
                    unityRigidbody.angularVelocity = ((Vector3D)o).Map();
                    break;
                case "angulardrag":
                    unityRigidbody.angularDrag = (float)o;
                    break;
                case "maxangularvelocity":
                    unityRigidbody.maxAngularVelocity = (float)o;
                    break;
                case "maxdepenetrationvelocity":
                    unityRigidbody.maxDepenetrationVelocity = (float)o;
                    break;
                case "collisiondetectionmode":
                    unityRigidbody.collisionDetectionMode = ConvertEnum<UnityEngine.CollisionDetectionMode>(o);
                    break;
                case "constraints":
                    unityRigidbody.constraints = ConvertEnum<UnityEngine.RigidbodyConstraints>(o);
                    break;
                default:
                    throw new Exception($"Property {n} is not setup");
            }
        }

        public void Update()
        {
            if (instance.Changed)
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

                foreach (var torque in instance.AdditionalTorques)
                {
                    if (torque.Relative)
                        unityRigidbody.AddRelativeTorque(torque.Torque.Map(), ConvertEnum<ForceMode>(torque.Mode));
                    else
                        unityRigidbody.AddTorque(torque.Torque.Map(), ConvertEnum<ForceMode>(torque.Mode));
                }

                instance.AdditionalForces.Clear();
                instance.AdditionalTorques.Clear();

                instance.ProcessedChanges();
            }
        }

        public void SetInstance(Rigidbody rigidbody)
        {
            instance = rigidbody;
            gameObject.SetActive(true);
        }

        public static Rigidbody NewInstance()
        {
            return new Rigidbody();
        }

        public TResult ConvertEnum<TResult>(object i) => (TResult)Enum.Parse(typeof(TResult), i.ToString(), true);
    }
}
