using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisCore.UI;
using SynthesisCore.Components;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.Utilities;

namespace SynthesisCore
{
    public class SampleSystem : SystemBase
    {
        private Entity testBody;

        private MotorManager motorManager;
        private MotorController frontLeft, frontRight, backLeft, backRight;
        private MotorController arm;

        public override void OnPhysicsUpdate() { }

        public override void Setup()
        {
            /*
            Entity e = EnvironmentManager.AddEntity();

            e.AddComponent<Transform>().Position = new Vector3D(0, .5, 5);
            Mesh m = e.AddComponent<Mesh>();
            cube(m);
            e.AddComponent<MeshCollider>();
            var selectable = e.AddComponent<Selectable>();
            selectable.OnSelect = () =>
            {
                EntityToolbar.Open(selectable.Entity.Value);
            };
            selectable.OnDeselect = () =>
            {
                EntityToolbar.Close();
            };
            e.AddComponent<Moveable>().Channel = 5;
            */
            
            testBody = EnvironmentManager.AddEntity();
            GltfAsset g = AssetManager.GetAsset<GltfAsset>("/modules/synthesis_core/Test.glb");
            Bundle o = g.Parse();
            testBody.AddBundle(o);

            var selectable = testBody.AddComponent<Selectable>();
            selectable.OnSelect = () =>
            {
                EntityToolbar.Open(selectable.Entity.Value);
            };
            selectable.OnDeselect = () =>
            {
                EntityToolbar.Close();
            };

            testBody.AddComponent<Moveable>().Channel = 5;

            var TestMotor = new MotorType() {
                MaxVelocity = 10000,
                Torque = 50,
                MotorName = "Testing Motor"
            };

            motorManager = testBody.AddComponent<MotorManager>();

            frontLeft = motorManager.AllMotorControllers[3];
            frontRight = motorManager.AllMotorControllers[4];
            backLeft = motorManager.AllMotorControllers[1];
            backRight = motorManager.AllMotorControllers[2];

            frontLeft.Gearing = 1.0f / 10.0f;
            frontLeft.MotorType = TestMotor;
            frontLeft.MotorCount = 1;
            frontLeft.Locked = true;
            frontRight.Gearing = 1.0f / 10.0f;
            frontRight.MotorType = TestMotor;
            frontRight.MotorCount = 1;
            frontRight.Locked = true;
            backLeft.Gearing = 1.0f / 10.0f;
            backLeft.MotorType = TestMotor;
            backLeft.MotorCount = 1;
            backLeft.Locked = true;
            backRight.Gearing = 1.0f / 10.0f;
            backRight.MotorType = TestMotor;
            backRight.MotorCount = 1;
            backRight.Locked = true;

            var ArmMotor = new MotorType() {
                MaxVelocity = 1000000000f,
                Torque = 3f,
                MotorName = "Arm Motor"
            };

            arm = motorManager.AllMotorControllers[0];
            arm.Gearing = 1.0f / 2.0f;
            arm.MotorType = ArmMotor;
            arm.MotorCount = 1;
            arm.Locked = true;
            arm.SetPercent(0);

            InputManager.AssignAxis("vert", new Analog("Vertical"));
            InputManager.AssignAxis("hori", new Analog("Horizontal"));
            InputManager.AssignDigitalInput("move_arm", new Digital("t"));
        }

        public override void OnUpdate() {
            float forward = InputManager.GetAxisValue("vert");
            float turn = InputManager.GetAxisValue("hori");
            
            /*
            frontLeft.SetPercent(forward + turn);
            frontRight.SetPercent(forward - turn);
            backLeft.SetPercent(forward + turn);
            backRight.SetPercent(forward - turn);
            */
        }

        [TaggedCallback("input/move_arm")]
        public void MoveArm(DigitalEvent e)
        {
            if(e.State == DigitalState.Held)
            {
                arm.SetPercent(1f);
            }
            else
            {
                arm.SetPercent(-1f);
            }
        }

        private Mesh cube(Mesh m)
        {
            if (m == null)
                m = new Mesh();

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
            return m;
        }

        public override void Teardown() { }
    }
}
