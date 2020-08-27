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
using SynthesisCore.Meshes;

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

            cube.AddComponent<Transform>().Position = new Vector3D(0, 5, 5);
            Mesh m = cube.AddComponent<Mesh>();
            Cube.Make(m);
            cube.AddComponent<MeshCollider>();
            cube.AddComponent<Rigidbody>();
            var cubeSelectable = cube.AddComponent<Selectable>();
            //e.AddComponent<Moveable>().Channel = 5;

            powerSupply = new PowerSupply(12); // V
            
            testBody = EnvironmentManager.AddEntity();

            GltfAsset g = AssetManager.GetAsset<GltfAsset>("/modules/synthesis_core/Test.glb");
            Bundle o = g.Parse();
            testBody.AddBundle(o);
            SynthesisCoreData.ModelsDict.Add("TestBody", testBody);
            var body = testBody.GetComponent<Joints>().AllJoints;

            var selectable = testBody.AddComponent<Selectable>();

            testBody.AddComponent<Moveable>().Channel = 5;

            motorManager = testBody.AddComponent<MotorAssemblyManager>();

            frontLeft = motorManager.AllMotorAssemblies[3];
            frontRight = motorManager.AllMotorAssemblies[4];
            backLeft = motorManager.AllMotorAssemblies[1];
            backRight = motorManager.AllMotorAssemblies[2];
            arm = motorManager.AllMotorAssemblies[0];

            frontLeft.Configure(MotorTypes.CIMMotor, gearReduction: 9.29);
            frontRight.Configure(MotorTypes.CIMMotor, gearReduction: 9.29);
            backLeft.Configure(MotorTypes.CIMMotor, gearReduction: 9.29);
            backRight.Configure(MotorTypes.CIMMotor, gearReduction: 9.29);
            arm.Configure(MotorTypes.CIMMotor, gearReduction: 9.29);

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

        public override void Teardown() { }

        public override void OnPhysicsUpdate() { }
    }
}
