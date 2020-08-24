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
using SynthesisCore.Simulation;

namespace SynthesisCore
{
    public class SampleSystem : SystemBase
    {
        private Entity testBody;

        private MotorAssemblyManager motorManager;
        private MotorAssembly frontLeft, frontRight, backLeft, backRight, arm;

        private PowerSupply powerSupply;

        // private const double LoadTorque = 22 * 9.81 * 1.1 * 0.0508; // m * g * μ * r
        
        public override void Setup()
        {
            Entity cube = EnvironmentManager.AddEntity();

            cube.AddComponent<Transform>().Position = new Vector3D(0, .5, 5);
            Mesh m = cube.AddComponent<Mesh>();
            MakeCube(m);
            cube.AddComponent<MeshCollider>();
            cube.AddComponent<Rigidbody>();
            var cubeSelectable = cube.AddComponent<Selectable>();
            cubeSelectable.OnSelect = () =>
            {
                EntityToolbar.Open(cubeSelectable.Entity.Value);
            };
            cubeSelectable.OnDeselect = () =>
            {
                EntityToolbar.Close();
            };
            //e.AddComponent<Moveable>().Channel = 5;

            powerSupply = new PowerSupply(12); // V
            
            testBody = EnvironmentManager.AddEntity();

            GltfAsset g = AssetManager.GetAsset<GltfAsset>("/modules/synthesis_core/Test.glb");
            Bundle o = g.Parse();
            testBody.AddBundle(o);
            SynthesisCoreData.ModelsDict.Add("TestBody", testBody);
            var body = testBody.GetComponent<Joints>().AllJoints;

            var selectable = testBody.AddComponent<Selectable>();
            cubeSelectable.OnSelect = () =>
            {
                EntityToolbar.Open(cubeSelectable.Entity.Value);
            };
            cubeSelectable.OnDeselect = () =>
            {
                EntityToolbar.Close();
            };

            testBody.AddComponent<Moveable>().Channel = 5;

            motorManager = testBody.AddComponent<MotorAssemblyManager>();

            frontLeft = motorManager.AllGearBoxes[3];
            frontRight = motorManager.AllGearBoxes[4];
            backLeft = motorManager.AllGearBoxes[1];
            backRight = motorManager.AllGearBoxes[2];
            arm = motorManager.AllGearBoxes[0];

            frontLeft.Configure(MotorTypes.CIMMotor, 9.29);
            frontRight.Configure(MotorTypes.CIMMotor, 9.29);
            backLeft.Configure(MotorTypes.CIMMotor, 9.29);
            backRight.Configure(MotorTypes.CIMMotor, 9.29);
            arm.Configure(MotorTypes.CIMMotor, 9.29);

            frontLeft.SetConstantLoadTorque(0.6050985);
            frontRight.SetConstantLoadTorque(0.6050985);
            backLeft.SetConstantLoadTorque(0.6050985);
            backRight.SetConstantLoadTorque(0.6050985);
            arm.SetConstantLoadTorque(0.6050985);

            InputManager.AssignAxis("vert", new Analog("Vertical"));
            InputManager.AssignAxis("hori", new Analog("Horizontal"));

            InputManager.AssignDigitalInput("move_arm_up", new Digital("t"));
            InputManager.AssignDigitalInput("move_arm_down", new Digital("y"));

            foreach (var i in EnvironmentManager.GetComponentsWhere<Rigidbody>(_ => true))
            {
                i.AngularDrag = 0;
            }

            //Entity jointTest = EnvironmentManager.AddEntity();
            //Mesh m = new Mesh();
            //Bundle b = new Bundle();
            //b.Components.Add(selectable);
            //Transform t = new Transform();
            //t.Position = new Vector3D(10, 10, 10); //joint anchor position
            //b.Components.Add(t);
            //b.Components.Add(cube(m)); //replace the cube function with your mesh creation
            //jointTest.AddBundle(b);
            ////when done
            //jointTest.RemoveEntity();
        }

        public override void OnUpdate()
        {
            float forward = InputManager.GetAxisValue("vert");
            float turn = InputManager.GetAxisValue("hori");

            var percent = forward + turn;
            frontLeft.SetVoltage(powerSupply.VoltagePercent(percent));
            percent = forward - turn;
            frontRight.SetVoltage(powerSupply.VoltagePercent(percent));
            percent = forward + turn;
            backLeft.SetVoltage(powerSupply.VoltagePercent(percent));
            percent = forward - turn;
            backRight.SetVoltage(powerSupply.VoltagePercent(percent));
            
            frontLeft.Update();
            frontRight.Update();
            backLeft.Update();
            backRight.Update();
            arm.Update();
        }

        [TaggedCallback("input/move_arm_up")]
        public void MoveArmUp(DigitalEvent e)
        {
            if(e.State == DigitalState.Held)
                arm.SetVoltage(powerSupply.VoltagePercent(0.25));
            else
                arm.SetVoltage(powerSupply.VoltagePercent(0));
        }

        [TaggedCallback("input/move_arm_down")]
        public void MoveArmDown(DigitalEvent e)
        {
            if (e.State == DigitalState.Held)
                arm.SetVoltage(powerSupply.VoltagePercent(-0.25));
            else
                arm.SetVoltage(powerSupply.VoltagePercent(0));
        }
        private Mesh MakeCube(Mesh m)
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

        public override void OnPhysicsUpdate() { }
    }
}
