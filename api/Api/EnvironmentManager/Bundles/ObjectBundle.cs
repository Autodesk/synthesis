using System;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.EnvironmentManager.Bundles
{
    public class ObjectBundle : IBundle
    {
        public UniqueTypeList<Component> Components { get; }
        public Mesh Mesh { get; set; }
        public Transform Transform { get; set; }

        public ObjectBundle()
        {
            Mesh = new Mesh();
            Transform = new Transform();
        }

        public void AddMesh()
        {
            // parse mesh
        }

        public void ParseMesh()
        {

        }

    }
}
