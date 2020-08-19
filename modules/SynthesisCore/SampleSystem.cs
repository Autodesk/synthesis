using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Modules.Attributes;
using SynthesisCore.Components;
using SynthesisCore.Systems;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;

namespace SynthesisCore
{
    public class SampleSystem : SystemBase
    {
        private MotorController frontLeft, frontRight, backLeft, backRight;
        private MotorController arm;

        public Mesh m;

        public override void OnPhysicsUpdate() { }

        public override void Setup()
        {
            Entity testBody = EnvironmentManager.AddEntity();
            GltfAsset g = AssetManager.GetAsset<GltfAsset>("/modules/synthesis_core/Test.glb");
            Bundle o = g.Parse();
            testBody.AddBundle(o);
            SynthesisCoreData.ModelsDict.Add("TestBody", testBody);
            var body = testBody.GetComponent<Joints>().AllJoints;

            (float r, float g, float b, float a)[] colors = {
                (1, 0, 0, 1),
                (0, 1, 0, 1),
                (0, 0, 1, 1),
                (1, 0, 1, 1)
            };

            var TestMotor = new MotorType() {
                MaxVelocity = 10000,
                Torque = 50,
                MotorName = "Testing Motor"
            };

            frontLeft = MotorManager.AllMotorControllers[3];
            frontRight = MotorManager.AllMotorControllers[4];
            backLeft = MotorManager.AllMotorControllers[1];
            backRight = MotorManager.AllMotorControllers[2];

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
                MaxVelocity = 10,
                Torque = 50,
                MotorName = "Arm Motor"
            };

            arm = MotorManager.AllMotorControllers[0];
            arm.Gearing = 1.0f / 2.0f;
            arm.MotorType = ArmMotor;
            arm.MotorCount = 1;
            arm.Locked = true;

            Digital[] test = { new Digital("w"), new Digital("a"), new Digital("s"), new Digital("d") };

            InputManager.AssignAxis("vert", new Analog("Vertical"));
            InputManager.AssignAxis("hori", new Analog("Horizontal"));

            Entity jointTest = EnvironmentManager.AddEntity();
            Bundle b = new Bundle();
            Selectable selectable = new Selectable();
            b.Components.Add(selectable);
            Transform t = new Transform();
            t.Position = new Vector3D(10, 10, 10); //joint anchor position
            b.Components.Add(t);
            b.Components.Add(sphere()); //replace the cube function with your mesh creation
            jointTest.AddBundle(b);
            //when done
            jointTest.RemoveEntity();
        }

        public override void OnUpdate() {
            float forward = InputManager.GetAxisValue("vert");
            float turn = InputManager.GetAxisValue("hori");
            
            var moveArmForward = new Digital("t");
            moveArmForward.Update();
            if (moveArmForward.State == DigitalState.Down) {
                arm.SetPercent(0.5f);
            } else {
                arm.SetPercent(0.0f);
            }

            frontLeft.SetPercent(forward + turn);
            frontRight.SetPercent(forward - turn);
            backLeft.SetPercent(forward + turn);
            backRight.SetPercent(forward - turn);
        }

        private Mesh cube()
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

        private Mesh sphere()
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
