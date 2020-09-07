using System;
using UnityEngine;
using SynthesisAPI.Utilities;
using MeshCollider = SynthesisAPI.EnvironmentManager.Components.MeshCollider;
using Mesh = SynthesisAPI.EnvironmentManager.Components.Mesh;
using MeshColliderCookingOptions = SynthesisAPI.EnvironmentManager.Components.MeshColliderCookingOptions;
using PhysicsMaterial = SynthesisAPI.EnvironmentManager.Components.PhysicsMaterial;
using Logger = SynthesisAPI.Utilities.Logger;
using System.ComponentModel;
using SynthesisAPI.EnvironmentManager;
using static Engine.ModuleLoader.Api;
using System.Collections.Generic;

namespace Engine.ModuleLoader.Adapters
{
    public class MeshColliderAdapter : MonoBehaviour, IApiAdapter<MeshCollider>
    {
        private static List<MeshColliderAdapter> allColliders = new List<MeshColliderAdapter>();

        internal UnityEngine.MeshCollider unityCollider;
        internal MeshCollider instance;

        public void OnDestroy()
        {
            allColliders.Remove(this);
        }

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

            foreach (MeshColliderAdapter adapter in allColliders)
            {
                if (adapter.instance.collisionLayer.Equals(instance.collisionLayer))
                {
                    Physics.IgnoreCollision(unityCollider, adapter.unityCollider);
                }
            }

            allColliders.Add(this);
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

        public void Update()
        {
            // Nothing to actively update
        }

        private MeshCollider.Collision MapCollision(Collision collision)
        {
            Entity? e = null;
            if (ApiProviderData.GameObjects.TryGetValue(collision.collider.transform.gameObject, out Entity otherE))
            {
                e = otherE;
            }
            return new MeshCollider.Collision(collision.impulse.Map(), collision.relativeVelocity.Map(), e);
        }

        public void OnCollisionEnter(Collision collision)
        {
            /*TagAdapter tagA, tagB;
            if ((tagA = collision.gameObject.GetComponent<TagAdapter>()) != null && (tagB = collision.gameObject.GetComponent<TagAdapter>()))
            {
                if (tagA.instance.tagName != "Untagged" && tagB.instance.tagName != "Untagged")
                {
                    if (tagA.instance.tagName == tagB.instance.tagName)
                    {
                        Physics.IgnoreCollision(collision.collider, unityCollider);
                        Logger.Log("Ignoring collision");
                    }
                }
            }
            else */if(instance != null && instance.OnCollisionEnter != null)
            {
                instance.OnCollisionEnter(MapCollision(collision));
            }
        }

        public void OnCollisionStay(Collision collision)
        {
            if (instance != null && instance.OnCollisionEnter != null)
            {
                instance.OnCollisionStay(MapCollision(collision));
            }
        }

        public void OnCollisionExit(Collision collision)
        {
            if (instance != null && instance.OnCollisionEnter != null)
            {
                instance.OnCollisionExit(MapCollision(collision));
            }
        }

        public static MeshCollider NewInstance()
        {
            return new MeshCollider();
        }

        private TResult ConvertEnum<TResult>(object i) => (TResult)Enum.Parse(typeof(TResult), i.ToString(), true);
    }
}
