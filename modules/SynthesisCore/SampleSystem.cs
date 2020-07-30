using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using SynthesisCore.Components;

namespace SynthesisCore
{
    [ModuleExport]
    public class SampleSystem : SystemBase
    {
        private Selectable selectable;
        private Transform transform;
        private Rigidbody rigidbody;

        private Entity A, B, C;

        public override void OnPhysicsUpdate() { }

        public override void Setup()
        {
            A = CreateTestEntity(new Vector3D(0, 2, 0));
            B = CreateTestEntity(new Vector3D(1.1f, 2, 0));
            C = CreateTestEntity(new Vector3D(1.1f, 3.1f, 0));

            var hingeJoint = B.AddComponent<HingeJoint>();
            hingeJoint.ConnectedBody = A.GetComponent<Rigidbody>();
            hingeJoint.Anchor = new Vector3D(0.55f, 0, 0);
            hingeJoint.Axis = new Vector3D(1, 0, 0);
            hingeJoint.BreakForce = 250;

            var hinge2 = C.AddComponent<HingeJoint>();
            hinge2.ConnectedBody = B.GetComponent<Rigidbody>();
            hinge2.Anchor = new Vector3D(0, -0.55f, 0);
            hinge2.Axis = new Vector3D(0, 1, 0);
            hinge2.BreakForce = 250;

            Digital[] test = { new Digital("w"), new Digital("a"), new Digital("s"), new Digital("d") };
            InputManager.AssignDigitalInputs("move", test);
            InputManager.AssignDigitalInput("test_move", new Digital("f"));
        }

        private static uint nextChannel = 5;
        private Entity CreateTestEntity(Vector3D pos)
        {
            var physMat = new PhysicsMaterial();
            // physMat.Bounciness = 1.0f;
            physMat.DynamicFriction = 0.05f;
            // physMat.BounceCombine = PhysicMaterialCombine.Maximum;

            Entity e = EnvironmentManager.AddEntity();
            transform = e.AddComponent<Transform>();
            transform.Position = pos;
            Mesh m = e.AddComponent<Mesh>();
            cube(m);
            var collider = e.AddComponent<MeshCollider>();
            collider.Material = physMat;
            selectable = e.AddComponent<Selectable>();
            e.AddComponent<Moveable>().Channel = nextChannel;
            nextChannel++;

            rigidbody = e.AddComponent<Rigidbody>();
            // rigidbody.Velocity = new Vector3D(10, 0, 0);
            // rigidbody.useGravity = false;
            rigidbody.Mass = 1.0f;
            rigidbody.MaxAngularVelocity = 1080.0f;

            return e;
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
                B.GetComponent<Rigidbody>().AddForce(new Vector3D(100, 200, 100));
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
