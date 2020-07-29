using System;
using UnityEngine;
using SynthesisAPI.Utilities;
using MeshCollider = SynthesisAPI.EnvironmentManager.Components.MeshCollider;
using Mesh = SynthesisAPI.EnvironmentManager.Components.Mesh;

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

            if ((unityCollider = GetComponent<UnityEngine.MeshCollider>()) == null)
                unityCollider = gameObject.AddComponent<UnityEngine.MeshCollider>();

            if ((mesh = GetComponent<MeshAdapter>()) == null)
                throw new Exception("No mesh adapter found");

            instance.SharedMesh = mesh.instance;
        }

        private void Update()
        {
            if (instance.Changed)
            {
                unityCollider.sharedMesh = instance.SharedMesh.ToUnity();
                unityCollider.convex = instance.Convex;
                unityCollider.cookingOptions = ConvertEnum<MeshColliderCookingOptions>(instance.CookingOptions);

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
