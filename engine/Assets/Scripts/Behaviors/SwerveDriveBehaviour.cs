using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Synthesis.PreferenceManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using UnityEngine;

namespace Synthesis {
    public class SwerveDriveBehaviour : SimBehaviour {
        
        internal const string FORWARD = "Swerve Forward";
        internal const string BACKWARD = "Swerve Backward";
        internal const string LEFT = "Swerve Left";
        internal const string RIGHT = "Swerve Right";
        internal const string TURN_LEFT = "Swerve Turn Left";
        internal const string TURN_RIGHT = "Swerve Turn Right";

        private List<RotationalDriver> _azimuthDrivers;
        private string[] _driveSignals;
        private RobotSimObject _robot;

        public SwerveDriveBehaviour(RobotSimObject robot, IEnumerable<RotationalDriver> azimuthSignals, string[] driveSignals) : base(robot.Name) {
            _azimuthDrivers = azimuthSignals.ToList();
            _driveSignals = driveSignals;

            _robot = robot;

            _azimuthDrivers.ForEach(x => {
                x.ControlMode = RotationalDriver.RotationalControlMode.Position;
                x.SetAxis(robot.GroundedNode.transform.up);
            });
            
            InitInputs(GetInputs());
            EventBus.NewTypeListener<ValueInputAssignedEvent>(OnValueInputAssigned);
        }
        
        public (string key, Analog input)[] GetInputs() {
            return new (string key, Analog input)[] {
                (FORWARD, TryLoadInput(FORWARD, new Digital("W"))),
                (BACKWARD, TryLoadInput(BACKWARD, new Digital("S"))),
                (LEFT, TryLoadInput(LEFT, new Digital("A"))),
                (RIGHT, TryLoadInput(RIGHT, new Digital("D"))),
                (TURN_LEFT, TryLoadInput(TURN_LEFT, new Digital("LeftArrow"))),
                (TURN_RIGHT, TryLoadInput(TURN_RIGHT, new Digital("RightArrow")))
            };
        }
        
        public Analog TryLoadInput(string key, Analog defaultInput) {
            return SimulationPreferences.GetRobotInput(
                       _robot.MiraLive.MiraAssembly.Info.GUID, key)
                   ?? defaultInput;
        }
        
        private void OnValueInputAssigned(IEvent tmp) {
            ValueInputAssignedEvent args = tmp as ValueInputAssignedEvent;
            switch (args.InputKey) {
                case FORWARD:
                case BACKWARD:
                case LEFT:
                case RIGHT: 
                case TURN_LEFT: 
                case TURN_RIGHT:
                    if (base.SimObjectId != RobotSimObject.GetCurrentlyPossessedRobot().MiraGUID) return;
                    RobotSimObject robot = SimulationManager.SimulationObjects[base.SimObjectId] as RobotSimObject;
                    SimulationPreferences.SetRobotInput(
                        _robot.MiraLive.MiraAssembly.Info.GUID,
                        args.InputKey,
                        args.Input);
                    break;
            }
        }

        public override void Update() {

            float forward = 0f;
            float strafe = 0f;
            float turn = 0f;
            
            var robotTransform = _robot.GroundedNode.transform;

            Vector3 chassisVelocity = robotTransform.forward * forward + robotTransform.right * strafe;
            Vector3 chassisAngularVelocity = robotTransform.up * turn;
            
            Vector3[] speeds = new Vector3[_azimuthDrivers.Count];
            for (int i = 0; i < _azimuthDrivers.Count; i++) {
                var driver = _azimuthDrivers[i];
                var com = robotTransform.localToWorldMatrix.MultiplyPoint3x4(robotTransform.GetComponent<Rigidbody>().centerOfMass);
                var radius = driver.Anchor - com;
                // Remove axis component of radius
                radius -= Vector3.Dot(driver.Axis, radius) * driver.Axis;

                speeds[i] = Vector3.Cross(chassisAngularVelocity, radius) + chassisVelocity;
            }
        }
    }
}
