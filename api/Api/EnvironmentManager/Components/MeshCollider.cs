using SynthesisAPI.Modules.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI.EnvironmentManager.Components
{
    [BuiltinComponent]
    public class MeshCollider : Component
    {
        #region Properties

        private bool _convex = true;
        private MeshColliderCookingOptions _cookingOptions = MeshColliderCookingOptions.None;
        private Mesh _sharedMesh = new Mesh();

        /// <summary>
        /// This is just here so when it detects changes it will default this
        /// </summary>
        public bool Convex {
            get => _convex;
        }

        public MeshColliderCookingOptions CookingOptions {
            get => _cookingOptions;
            set {
                _cookingOptions = value;
                Changed = true;
            }
        }

        public Mesh SharedMesh {
            get => _sharedMesh;
            set {
                _sharedMesh = value;
                Changed = true;
            }
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
}
