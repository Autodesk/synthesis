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

        private Entity A, B, C, D, E;

        public override void OnPhysicsUpdate() { }

        public override void Setup()
        {
            A = CreateTestEntity(new Vector3D(0, 2f, 0));
            B = CreateTestEntity(new Vector3D(1f, 1f, 0));
            C = CreateTestEntity(new Vector3D(0, 2f, 1f));
            D = CreateTestEntity(new Vector3D(-1f, 2f, 0));
            E = CreateTestEntity(new Vector3D(0, 2f, -1f));

            #region Hard-coded joints for demonstration

            /*var hingeJoint = B.AddComponent<HingeJoint>();
            hingeJoint.ConnectedBody = A.GetComponent<Rigidbody>();
            hingeJoint.Anchor = new Vector3D(-0.5f, 0.5f, 0);
            hingeJoint.Axis = new Vector3D(0, 0, 1);
            hingeJoint.BreakForce = 500;
            hingeJoint.UseLimits = true;
            hingeJoint.Limits = new JointLimits()
            {
                Max = 90,
                Min = -90
            };*/

            var h1 = B.AddComponent<FixedJoint>();
            h1.ConnectedBody = A.GetComponent<Rigidbody>();
            h1.BreakForce = 500;

            var h2 = C.AddComponent<HingeJoint>();
            h2.ConnectedBody = A.GetComponent<Rigidbody>();
            h2.Anchor = new Vector3D(0, -0.5f, -0.5f);
            h2.Axis = new Vector3D(1, 0, 0);
            h2.BreakForce = 500;
            h2.UseLimits = true;
            h2.Limits = new JointLimits()
            {
                Max = 180,
                Min = 0 // Might need to switch
            };

            var h3 = D.AddComponent<HingeJoint>();
            h3.ConnectedBody = A.GetComponent<Rigidbody>();
            h3.Anchor = new Vector3D(0.5f, -0.5f, 0);
            h3.Axis = new Vector3D(0, 0, 1);
            h3.BreakForce = 500;
            h3.UseLimits = true;
            h3.Limits = new JointLimits()
            {
                Max = 180,
                Min = 0 // Might need to switch
            };

            var h4 = E.AddComponent<HingeJoint>();
            h4.ConnectedBody = A.GetComponent<Rigidbody>();
            h4.Anchor = new Vector3D(0, -0.5f, 0.5f);
            h4.Axis = new Vector3D(1, 0, 0);
            h4.BreakForce = 500;
            h4.UseLimits = true;
            h4.Limits = new JointLimits()
            {
                Max = 0,
                Min = -180 // Might need to switch
            };

            #endregion

            Digital[] test = { new Digital("w"), new Digital("a"), new Digital("s"), new Digital("d") };
            InputManager.AssignDigitalInputs("move", test);
            InputManager.AssignDigitalInput("fly", new Digital("f"));
            InputManager.AssignDigitalInput("spin", new Digital("r"));
            InputManager.AssignDigitalInput("break", new Digital("x"));
        }

        private static uint nextChannel = 5;
        private Entity CreateTestEntity(Vector3D pos)
        {
            var physMat = new PhysicsMaterial();
            // physMat.Bounciness = 1.0f;
            // physMat.DynamicFriction = 0.05f;
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
            rigidbody.MaxAngularVelocity = 400.0f;
            rigidbody.Drag = 0.1f;

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

        [TaggedCallback("input/fly")]
        public void Fly(DigitalEvent de)
        {
            if (de.State == DigitalState.Down)
            {
                // ApiProvider.Log("Key Pressed");
                Selectable.Selected?.Entity?.GetComponent<Rigidbody>().AddForce(new Vector3D(0, 475, 0));
            }
        }

        [TaggedCallback("input/spin")]
        public void Spin(DigitalEvent de)
        {
            if (de.State == DigitalState.Down)
            {
                // ApiProvider.Log("Key Pressed");
                Selectable.Selected?.Entity?.GetComponent<Rigidbody>().AddTorque(new Vector3D(350, 0, 0));
            }
        }

        [TaggedCallback("input/break")]
        public void Break(DigitalEvent de)
        {
            if (de.State == DigitalState.Down)
            {
                // ApiProvider.Log("Key Pressed");
                B.GetComponent<FixedJoint>().BreakForce = 0f;
                C.GetComponent<HingeJoint>().BreakForce = 0f;
                D.GetComponent<HingeJoint>().BreakForce = 0f;
                E.GetComponent<HingeJoint>().BreakForce = 0f;
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
