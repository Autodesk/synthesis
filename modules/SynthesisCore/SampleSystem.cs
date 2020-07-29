using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;

namespace SynthesisCore
{
    [ModuleExport]
    public class SampleSystem : SystemBase
    {
        private Selectable selectable;
        private Transform transform;
        private Rigidbody rigidbody;

        public override void OnPhysicsUpdate() { }

        public override void Setup()
        {
            Entity e = EnvironmentManager.AddEntity();
            transform = e.AddComponent<Transform>();
            transform.Position = new Vector3D(0, 4, 0);
            Mesh m = e.AddComponent<Mesh>();
            cube(m);
            e.AddComponent<MeshCollider>();
            selectable = e.AddComponent<Selectable>();

            rigidbody = e.AddComponent<Rigidbody>();
            // rigidbody.Velocity = new Vector3D(10, 0, 0);
            rigidbody.useGravity = true;
            rigidbody.Mass = 500.0f;
            rigidbody.MaxAngularVelocity = 1080.0f;

            Digital[] test = { new Digital("w"), new Digital("a"), new Digital("s"), new Digital("d") };
            InputManager.AssignDigitalInputs("move", test);
            InputManager.AssignDigitalInput("test_move", new Digital("f"));
        }

        public override void OnUpdate() { }

        private void cube(Mesh m)
        {
            m.Vertices = new List<Vector3D>()
            {
                new Vector3D(-0.5,-0.5,-0.5),
                new Vector3D(0.5,-0.5,-0.5),
                new Vector3D(0.5,0.5,-0.5),
                new Vector3D(-0.5,0.5,-0.5),
                new Vector3D(-0.5,0.5,0.5),
                new Vector3D(0.5,0.5,0.5),
                new Vector3D(0.5,-0.5,0.5),
                new Vector3D(-0.5,-0.5,0.5)
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
        }

        [TaggedCallback("input/test_move")]
        public void TestMove(DigitalEvent de)
        {
            if (de.State == DigitalState.Down)
            {
                ApiProvider.Log("Key Pressed");
                rigidbody.AddTorque(new Vector3D(0, 0, 150));
            }
        }

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
