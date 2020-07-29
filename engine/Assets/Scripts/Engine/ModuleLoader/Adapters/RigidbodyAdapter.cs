using System;
using Rigidbody = SynthesisAPI.EnvironmentManager.Components.Rigidbody;
using UnityEngine;
using SynthesisAPI.Utilities;
using MathNet.Spatial.Euclidean;

namespace Engine.ModuleLoader.Adapters
{
    public class RigidbodyAdapter : MonoBehaviour, IApiAdapter<Rigidbody>
    {
        private Rigidbody instance;
        private UnityEngine.Rigidbody unityRigidbody;

        public void OnEnable()
        {
            if (instance == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if ((unityRigidbody = GetComponent<UnityEngine.Rigidbody>()) == null)
                unityRigidbody = gameObject.AddComponent<UnityEngine.Rigidbody>();

            // Setup linked getter and setter;
            instance.LinkedGetter = n => ParseFromUnity(typeof(UnityEngine.Rigidbody).GetProperty(n).GetGetMethod().Invoke(unityRigidbody, null));
            instance.LinkedSetter = (n, o) => typeof(UnityEngine.Rigidbody).GetProperty(n).GetSetMethod().Invoke(unityRigidbody, new object[] { ParseToUnity(o) });
        }

        private object ParseFromUnity(object obj)
        {
            Type type = obj.GetType();
            if (type.IsEnum)
            {
                return typeof(RigidbodyAdapter).GetMethod("ConvertEnum").MakeGenericMethod(Array.Find(typeof(Rigidbody).Assembly.GetTypes(),
                    x => x.Name == type.Name && x.FullName != type.FullName)).Invoke(this, new object[] { obj }); // Eh?
            }
            switch (type.Name)
            {
                case "Vector3":
                    return ((Vector3)obj).Map();
            }
            return obj;
        }

        private object ParseToUnity(object obj)
        {
            Type type = obj.GetType();
            if (type.IsEnum)
            {
                return typeof(RigidbodyAdapter).GetMethod("ConvertEnum").MakeGenericMethod(Array.Find(typeof(UnityEngine.Rigidbody).Assembly.GetTypes(),
                    x => x.Name == type.Name && x.FullName != type.FullName)).Invoke(this, new object[] { obj });
            }
            switch (type.Name)
            {
                case "Vector3D":
                    return ((Vector3D)obj).Map();
            }
            return obj;
        }

        public void Update()
        {
            if (instance.Changed)
            {
                /*unityRigidbody.isKinematic = instance.IsKinematic;
                unityRigidbody.mass = instance.Mass;
                unityRigidbody.velocity = instance.Velocity.Map();
                unityRigidbody.drag = instance.Drag;
                unityRigidbody.angularVelocity = instance.AngularVelocity.Map();
                unityRigidbody.angularDrag = instance.AngularDrag;
                unityRigidbody.collisionDetectionMode = ConvertEnum<CollisionDetectionMode>(instance.CollisionDetectionMode);*/
                
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
