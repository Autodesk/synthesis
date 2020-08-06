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
        internal MeshAdapter mesh;
        internal UnityEngine.MeshCollider unityCollider;
        internal MeshCollider instance;

        public void SetInstance(MeshCollider collider)
        {
            instance = collider;

            if ((unityCollider = GetComponent<UnityEngine.MeshCollider>()) == null)
                unityCollider = gameObject.AddComponent<UnityEngine.MeshCollider>();

            if ((mesh = GetComponent<MeshAdapter>()) == null)
                throw new Exception("No mesh adapter found");

            instance.PropertyChanged += UnityProperty;

            unityCollider.convex = instance.convex;

            if (instance.sharedMesh == null)
            {
                unityCollider.sharedMesh = EnvironmentManager.GetComponent<Mesh>(instance.Entity.Value).ToUnity();
                instance.sharedMesh = new Mesh(unityCollider.sharedMesh);
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
