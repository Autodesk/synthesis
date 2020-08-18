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

        private GearAssemblyManager motorManager;
        private MotorAssembly frontLeft, frontRight, backLeft, backRight;
        private MotorAssembly arm;

        private PowerSupply powerSupply;
        private DCMotor testMotor;

        private const double LoadTorque = 22 * 9.81 * 1.1 * 0.0508; // m * g * μ * r

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

            motorManager = testBody.AddComponent<GearAssemblyManager>();

            frontLeft = motorManager.AllGearBoxes[3];
            frontRight = motorManager.AllGearBoxes[4];
            backLeft = motorManager.AllGearBoxes[1];
            backRight = motorManager.AllGearBoxes[2];
            arm = motorManager.AllGearBoxes[0];

            InputManager.AssignAxis("vert", new Analog("Vertical"));
            InputManager.AssignAxis("hori", new Analog("Horizontal"));
            InputManager.AssignDigitalInput("move_arm_up", new Digital("t"));
            InputManager.AssignDigitalInput("move_arm_down", new Digital("y"));

            powerSupply = new PowerSupply(12); // V
            testMotor = MotorFactory.CIMMotor();
        }

        public override void OnUpdate() {
            float forward = InputManager.GetAxisValue("vert");
            float turn = InputManager.GetAxisValue("hori");

            var percent = forward + turn;
            frontLeft.Update(powerSupply.VoltagePercent(percent), System.Math.Sign(percent) * LoadTorque);
            percent = forward - turn;
            frontRight.Update(powerSupply.VoltagePercent(percent), System.Math.Sign(percent) * LoadTorque);
            percent = forward + turn;
            backLeft.Update(powerSupply.VoltagePercent(percent), System.Math.Sign(percent) * LoadTorque);
            percent = forward - turn;
            backRight.Update(powerSupply.VoltagePercent(percent), System.Math.Sign(percent) * LoadTorque);

            double percentVoltage = -1.0;

            testMotor.Update(percentVoltage * powerSupply.Voltage, 2.5);

            Logger.Log($"Torque: {(backRight.Motor.Torque * 1000)} m-Nm,   AngularSpeed: {backRight.Motor.AngularSpeed.RotationsPerMin} rpm,   " +
                       $"Current: {backRight.Motor.Current} Amps,     Load: {System.Math.Sign(percent) * LoadTorque * 1000} m-Nm", LogLevel.Debug);
        }

        [TaggedCallback("input/move_arm_up")]
        public void MoveArmUp(DigitalEvent e)
        {
            if(e.State == DigitalState.Held)
            {
                arm.Update(powerSupply.VoltagePercent(0.25), 0);
            }
            else
            {
                arm.Update(powerSupply.VoltagePercent(0), 0);
            }
        }

        [TaggedCallback("input/move_arm_down")]
        public void MoveArmDown(DigitalEvent e)
        {
            if (e.State == DigitalState.Held)
            {
                arm.Update(powerSupply.VoltagePercent(-0.25), 0);
            }
            else
            {
                arm.Update(powerSupply.VoltagePercent(0), 0);
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
