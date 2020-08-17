using System;
using UnityEngine;
using SynthesisAPI.Utilities;
using MeshCollider = SynthesisAPI.EnvironmentManager.Components.MeshCollider;
using Mesh = SynthesisAPI.EnvironmentManager.Components.Mesh;
using MeshColliderCookingOptions = SynthesisAPI.EnvironmentManager.Components.MeshColliderCookingOptions;
using PhysicsMaterial = SynthesisAPI.EnvironmentManager.Components.PhysicsMaterial;
using System.ComponentModel;
using SynthesisAPI.EnvironmentManager;

namespace Engine.ModuleLoader.Adapters
{
    public class MeshColliderAdapter : MonoBehaviour, IApiAdapter<MeshCollider>
    {
        internal UnityEngine.MeshCollider unityCollider;
        internal MeshCollider instance;

        public void SetInstance(MeshCollider collider)
        {
            instance = collider;

            if ((unityCollider = GetComponent<UnityEngine.MeshCollider>()) == null)
                unityCollider = gameObject.AddComponent<UnityEngine.MeshCollider>();

            if (instance.Entity?.GetComponent<Mesh>() == null)
                throw new SynthesisException("Cannot add a MeshCollider to an entity without a Mesh");

            instance.PropertyChanged += UnityProperty;

            unityCollider.convex = instance.convex;

            if (instance.sharedMesh == null)
            {
                instance.sharedMesh = instance.Entity?.GetComponent<Mesh>();
                unityCollider.sharedMesh = instance.sharedMesh.ToUnity();
            } else
            {
                unityCollider.sharedMesh = instance.sharedMesh.ToUnity();
            }

            if (instance.material != null)
            {
                unityCollider.material = instance.material.GetUnity();
            }

            unityCollider.cookingOptions = instance.cookingOptions.Convert<UnityEngine.MeshColliderCookingOptions>();
            
        }

        private void UnityProperty(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName.ToLower())
            {
                case "cookingoptions":
                    unityCollider.cookingOptions = instance.cookingOptions.Convert<UnityEngine.MeshColliderCookingOptions>();
                    break;
                case "sharedmesh":
                    unityCollider.sharedMesh = instance.sharedMesh.ToUnity();
                    break;
                case "material":
                    unityCollider.material = instance.material.GetUnity();
                    break;
                default:
                    throw new Exception($"Property {args.PropertyName} is not setup");
            }
        }

        public void Start()
        {
            gameObject.transform.position = gameObject.transform.position + new Vector3(0, float.Epsilon, 0); // Enable Unity collider by moving transform slightly
        }

        private void Update()
        {
            // Nothing to actively update
        }

        public static MeshCollider NewInstance()
        {
            return new MeshCollider();
        }

        private TResult ConvertEnum<TResult>(object i) => (TResult)Enum.Parse(typeof(TResult), i.ToString(), true);
    }
}
