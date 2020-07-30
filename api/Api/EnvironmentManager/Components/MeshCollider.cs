using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI.EnvironmentManager.Components
{
    [BuiltinComponent]
    public class MeshCollider : Component
    {
        #region Properties

        internal Action<string, object> LinkedSetter = (n, o) => throw new Exception("Setter not assigned");
        internal Func<string, object> LinkedGetter = n => throw new Exception("Getter not assigned");

        private void Set(string name, object obj) => LinkedSetter(name, obj);
        private T Get<T>(string name) => (T)LinkedGetter(name);

        /// <summary>
        /// This is just here so when it detects changes it will default this
        /// </summary>
        public bool Convex {
            get => Get<bool>("convex");
            set => Set("convex", value);
        }
        public MeshColliderCookingOptions CookingOptions {
            get => Get<MeshColliderCookingOptions>("cookingoptions");
            set => Set("cookingoptions", value);
        }
        public Mesh SharedMesh {
            get => Get<Mesh>("sharedmesh");
            set => Set("sharedmesh", value);
        }
        public PhysicsMaterial Material {
            get => Get<PhysicsMaterial>("material");
            set => Set("material", value);
        }

        #endregion

        internal bool Changed { get; private set; } = true;
        internal void ProcessedChanges() => Changed = false;
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
