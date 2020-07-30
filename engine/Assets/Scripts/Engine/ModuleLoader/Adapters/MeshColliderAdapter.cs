using System;
using UnityEngine;
using SynthesisAPI.Utilities;
using MeshCollider = SynthesisAPI.EnvironmentManager.Components.MeshCollider;
using Mesh = SynthesisAPI.EnvironmentManager.Components.Mesh;
using MeshColliderCookingOptions = SynthesisAPI.EnvironmentManager.Components.MeshColliderCookingOptions;
using PhysicsMaterial = SynthesisAPI.EnvironmentManager.Components.PhysicsMaterial;

namespace Engine.ModuleLoader.Adapters
{
    public class MeshColliderAdapter : MonoBehaviour, IApiAdapter<MeshCollider>
    {
        internal MeshAdapter mesh;
        internal UnityEngine.MeshCollider unityCollider;
        internal MeshCollider instance;

        private void OnEnable()
        {
            if (instance == null)
            {
                gameObject.SetActive(false);
                return;
            }

            instance.LinkedGetter = Getter;
            instance.LinkedSetter = Setter;

            if ((unityCollider = GetComponent<UnityEngine.MeshCollider>()) == null)
                unityCollider = gameObject.AddComponent<UnityEngine.MeshCollider>();

            if ((mesh = GetComponent<MeshAdapter>()) == null)
                throw new Exception("No mesh adapter found");

            instance.SharedMesh = mesh.instance;
            instance.Convex = true;
        }

        private object Getter(string n)
        {
            switch (n.ToLower())
            {
                case "convex":
                    return unityCollider.convex;
                case "cookingoptions":
                    return unityCollider.cookingOptions.Convert<MeshColliderCookingOptions>();
                case "sharedmesh":
                    return Mesh.FromUnity(unityCollider.sharedMesh);
                case "material":
                    return new PhysicsMaterial(unityCollider.material);
                default:
                    throw new Exception($"Property {n} is not setup");
            }
        }

        private void Setter(string n, object o)
        {
            switch (n.ToLower())
            {
                case "convex":
                    unityCollider.convex = (bool)o;
                    break;
                case "cookingoptions":
                    unityCollider.cookingOptions = ((MeshColliderCookingOptions)o).Convert<UnityEngine.MeshColliderCookingOptions>();
                    break;
                case "sharedmesh":
                    unityCollider.sharedMesh = ((Mesh)o).ToUnity();
                    break;
                case "material":
                    unityCollider.material = ((PhysicsMaterial)o).GetUnity();
                    break;
                default:
                    throw new Exception($"Property {n} is not setup");
            }
        }

        private void Update()
        {
            if (instance.Changed)
            {
                instance.ProcessedChanges();
            }
        }

        public void SetInstance(MeshCollider collider)
        {
            instance = collider;
            gameObject.SetActive(true);
        }

        public static MeshCollider NewInstance()
        {
            return new MeshCollider();
        }

        private TResult ConvertEnum<TResult>(object i) => (TResult)Enum.Parse(typeof(TResult), i.ToString(), true);
    }
}
