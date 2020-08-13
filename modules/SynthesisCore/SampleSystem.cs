using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Modules.Attributes;
using SynthesisCore.Components;
using SynthesisCore.Systems;

namespace SynthesisCore
{
    public class SampleSystem : SystemBase
    {
        public override void OnPhysicsUpdate() { }

        public override void Setup()
        {
            Entity e = EnvironmentManager.AddEntity();

            /*
            GltfAsset g = AssetManager.GetAsset<GltfAsset>("/modules/synthesis_core/Test.glb");
            var _m1 = new ProfilerMarker();
            Bundle o = g.Parse();
            Logger.Log($"Parse Time: {_m1.TimeSinceCreation.TotalMilliseconds}");
            var _m2 = new ProfilerMarker();
            e.AddBundle(o);
            Logger.Log($"Spawn Time: {_m2.TimeSinceCreation.TotalMilliseconds}");
            */

            e.AddComponent<Transform>().Position = new Vector3D(0, 0, 5);
            var selectable = e.AddComponent<Selectable>();
            selectable.OnSelect = () =>
            {
                MoveArrows.MoveEntity(selectable.Entity.Value);
            };
            selectable.OnDeselect = () =>
            {
                MoveArrows.StopMovingEntity();
            };
            e.AddComponent<Moveable>().Channel = 5;
            Mesh m = e.AddComponent<Mesh>();
            cube(m);
        }

        public override void OnUpdate() { }

        private Mesh cube(Mesh? m)
        {
            if (m == null)
                m = new Mesh();

            m.Vertices = new List<Vector3D>()
            {
                new Vector3D(0,0,0),
                new Vector3D(1,0,0),
                new Vector3D(1,1,0),
                new Vector3D(0,1,0),
                new Vector3D(0,1,1),
                new Vector3D(1,1,1),
                new Vector3D(1,0,1),
                new Vector3D(0,0,1)
            };
            m.Triangles = new List<int>()
            {
                0, 2, 1, //face front
			    0, 3, 2,
                2, 3, 4, //face top
			    2, 4, 5,
                1, 2, 5, //face right
			    1, 5, 6,
                0, 7, 4, //face left
			    0, 4, 3,
                5, 4, 7, //face back
			    5, 7, 6,
                0, 6, 7, //face bottom
			    0, 1, 6
            };
            return m;
        }

        public override void Teardown() { }

        /*
        [TaggedCallback("input/move")]
        public void Move(DigitalEvent digitalEvent)
        {
            if(digitalEvent.State == DigitalState.Held)
            {
                switch (digitalEvent.Name)
                {
                    case "w":
                        ApiProvider.Log("w");
                        break;
                    case "a":
                        ApiProvider.Log("a");
                        break;
                    case "s":
                        ApiProvider.Log("s");
                        break;
                    case "d":
                        ApiProvider.Log("d");
                        break;
                    default:
                        break;
                }
            }
        }
        */
    }
}
