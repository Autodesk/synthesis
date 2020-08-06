using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SynthesisAPI.EnvironmentManager.Components
{
    [BuiltinComponent]
    public class MeshCollider : Component
    {
        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        internal bool convex = true;
        /// <summary>
        /// This is just here so when it detects changes it will default this
        /// </summary>
        public bool Convex {
            get => convex;
        }
        internal MeshColliderCookingOptions cookingOptions = MeshColliderCookingOptions.UseFastMidphase
            | MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning
            | MeshColliderCookingOptions.WeldColocatedVertices;
        public MeshColliderCookingOptions CookingOptions {
            get => cookingOptions;
            set {
                cookingOptions = value;
                OnPropertyChanged();
            }
        }
        internal Mesh sharedMesh = null; // If mesh is null, it will attempt to grab from the MeshAdapter
        public Mesh SharedMesh {
            get => sharedMesh;
            set {
                sharedMesh = value;
                OnPropertyChanged();
            }
        }
        internal PhysicsMaterial material = new PhysicsMaterial(); // TODO: Some default friction and bounce values?
        public PhysicsMaterial Material {
            get => material;
            set {
                material = value;
                OnPropertyChanged();
            }
        }

        #endregion

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public enum MeshColliderCookingOptions
    {
        None = 0x0, CookForFasterSimulation = 0x2, EnableMeshCleaning = 0x4,
        WeldColocatedVertices = 0x8, UseFastMidphase = 0x10
    }

    public enum PhysicMaterialCombine
    {
        Average = 0,
        Minimum = 2,
        Multiply = 1,
        Maximum = 3
    }

    public class PhysicsMaterial
    {
        private UnityEngine.PhysicMaterial _container;

        public float Bounciness {
            get => _container.bounciness;
            set => _container.bounciness = value;
        }
        public float DynamicFriction {
            get => _container.dynamicFriction;
            set => _container.dynamicFriction = value;
        }
        public float StaticFriction {
            get => _container.staticFriction;
            set => _container.staticFriction = value;
        }
        public PhysicMaterialCombine FrictionCombine {
            get => _container.frictionCombine.Convert<PhysicMaterialCombine>();
            set => _container.frictionCombine = value.Convert<UnityEngine.PhysicMaterialCombine>();
        }
        public PhysicMaterialCombine BounceCombine {
            get => _container.bounceCombine.Convert<PhysicMaterialCombine>();
            set => _container.bounceCombine = value.Convert<UnityEngine.PhysicMaterialCombine>();
        }

        public PhysicsMaterial()
        {
            _container = new UnityEngine.PhysicMaterial();
        }

        internal PhysicsMaterial(UnityEngine.PhysicMaterial container)
        {
            _container = container;
        }

        internal UnityEngine.PhysicMaterial GetUnity() => _container;
    }
}
