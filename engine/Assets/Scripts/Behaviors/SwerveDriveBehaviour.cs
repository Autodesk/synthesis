using System;
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
        internal const string FORWARD             = "Swerve Forward";
        internal const string BACKWARD            = "Swerve Backward";
        internal const string LEFT                = "Swerve Left";
        internal const string RIGHT               = "Swerve Right";
        internal const string TURN_LEFT           = "Swerve Turn Left";
        internal const string TURN_RIGHT          = "Swerve Turn Right";
        internal const string RESET_FIELD_FORWARD = "Swerve Reset Forward";

        private (RotationalDriver azimuth, WheelDriver drive)[] _moduleDrivers;
        private RobotSimObject _robot;

        private float _turnFavor = 1.5f;

        private Vector3 _fieldForward;

        public SwerveDriveBehaviour(RobotSimObject robot, (RotationalDriver azimuth, WheelDriver drive)[] moduleDrivers)
            : base(robot.Name) {
            _moduleDrivers = moduleDrivers;

            _robot        = robot;
            _fieldForward = Vector3.forward;

            _moduleDrivers.ForEach(x => {
                if (x.azimuth.IsReserved) {
                    SimulationManager.RemoveBehaviour(_robot.Name, x.azimuth.Reservee);
                }

                x.azimuth.Reserve(this);
                x.azimuth.ControlMode = RotationalDriver.RotationalControlMode.Position;
                x.azimuth.SetAxis(robot.GroundedNode.transform.up);
            });

            InitInputs(GetInputs());
            EventBus.NewTypeListener<ValueInputAssignedEvent>(OnValueInputAssigned);
        }

        public (string key, string name, Analog input)[] GetInputs() {
            return new(
                string key, string name, Analog input)[] { (FORWARD, FORWARD, TryLoadInput(FORWARD, new Digital("W"))),
                (BACKWARD, BACKWARD, TryLoadInput(BACKWARD, new Digital("S"))),
                (LEFT, LEFT, TryLoadInput(LEFT, new Digital("A"))),
                (RIGHT, RIGHT, TryLoadInput(RIGHT, new Digital("D"))),
                (TURN_LEFT, TURN_LEFT, TryLoadInput(TURN_LEFT, new Digital("LeftArrow"))),
                (TURN_RIGHT, TURN_RIGHT, TryLoadInput(TURN_RIGHT, new Digital("RightArrow"))),
                (RESET_FIELD_FORWARD, RESET_FIELD_FORWARD, TryLoadInput(RESET_FIELD_FORWARD, new Digital("R"))) };
        }

        public Analog TryLoadInput(string key, Analog defaultInput) {
            return SimulationPreferences.GetRobotInput(_robot.MiraLive.MiraAssembly.Info.GUID, key) ?? defaultInput;
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
                case RESET_FIELD_FORWARD:
                    if (base.SimObjectId != RobotSimObject.GetCurrentlyPossessedRobot().MiraGUID)
                        return;
                    RobotSimObject robot = SimulationManager.SimulationObjects[base.SimObjectId] as RobotSimObject;
                    SimulationPreferences.SetRobotInput(
                        _robot.MiraLive.MiraAssembly.Info.GUID, args.InputKey, args.Input);
                    break;
            }
        }

        /// <summary>
        /// A difference comparison with some wiggle room between the values
        /// </summary>
        /// <param name="a">Value A</param>
        /// <param name="b">Value B</param>
        /// <param name="acceptableDelta">Allowed difference between them to be considered the same essentially.
        /// Exclusive</param> <returns></returns>
        private bool Diff(float a, float b, float acceptableDelta) {
            return Math.Abs(a - b) < acceptableDelta;
        }

        public override void Update() {
            if (Mathf.Abs(InputManager.MappedValueInputs[RESET_FIELD_FORWARD].Value) > 0.5f)
                _fieldForward = _robot.GroundedNode.transform.forward;

            Vector3 headingVector = _robot.GroundedNode.transform.forward -
                                    (Vector3.up * Vector3.Dot(Vector3.up, _robot.GroundedNode.transform.forward));
            float headingVectorY = Vector3.Dot(_fieldForward, headingVector);
            float headingVectorX = Vector3.Dot(Vector3.Cross(_fieldForward, Vector3.up), headingVector);
            float chassisAngle   = Mathf.Atan2(headingVectorX, headingVectorY) * Mathf.Rad2Deg;

            var forwardInput   = InputManager.MappedValueInputs[FORWARD];
            var backwardInput  = InputManager.MappedValueInputs[BACKWARD];
            var leftInput      = InputManager.MappedValueInputs[LEFT];
            var rightInput     = InputManager.MappedValueInputs[RIGHT];
            var turnLeftInput  = InputManager.MappedValueInputs[TURN_LEFT];
            var turnRightInput = InputManager.MappedValueInputs[TURN_RIGHT];

            float forward = Mathf.Abs(forwardInput.Value) - Mathf.Abs(backwardInput.Value);
            float strafe  = Mathf.Abs(rightInput.Value) - Mathf.Abs(leftInput.Value);
            float turn    = Mathf.Abs(turnRightInput.Value) - Mathf.Abs(turnLeftInput.Value);

            forward = Diff(forward, 0f, 0.1f) ? 0f : forward;
            strafe  = Diff(strafe, 0f, 0.1f) ? 0f : strafe;
            turn    = Diff(turn, 0f, 0.1f) ? 0f : turn;

            // Are the inputs basically zero
            if (forward == 0f && turn == 0f && strafe == 0f) {
                _moduleDrivers.ForEach(x => x.drive.MainInput = 0f);
                return;
            }

            // Adjusts how much turning verse translation is favored
            turn *= _turnFavor;

            var robotTransform = _robot.GroundedNode.transform;

            Vector3 chassisVelocity        = robotTransform.forward * forward + robotTransform.right * strafe;
            Vector3 chassisAngularVelocity = robotTransform.up * turn;

            // Normalize velocity so its between 1 and 0. Should only max out at like 1 sqrt(2), but still
            if (chassisVelocity.magnitude > 1)
                chassisVelocity = chassisVelocity.normalized;

            // Rotate chassis velocity by chassis angle
            chassisVelocity = Quaternion.AngleAxis(chassisAngle, robotTransform.up) * chassisVelocity;

            var maxVelocity      = Vector3.zero;
            Vector3[] velocities = new Vector3[_moduleDrivers.Length];
            for (int i = 0; i < velocities.Length; i++) {
                // TODO: We should do this only once for all azimuth drivers, but whatever for now
                var driver = _moduleDrivers[i].azimuth;
                var com    = robotTransform.localToWorldMatrix.MultiplyPoint3x4(
                    robotTransform.GetComponent<Rigidbody>().centerOfMass);
                var radius = driver.Anchor - com;
                // Remove axis component of radius
                radius -= Vector3.Dot(driver.Axis, radius) * driver.Axis;

                velocities[i] = Vector3.Cross(chassisAngularVelocity, radius) + chassisVelocity;
                if (velocities[i].magnitude > maxVelocity.magnitude)
                    maxVelocity = velocities[i];
            }

            // Normalize all if a velocity exceeds 1
            if (maxVelocity.magnitude > 1) {
                for (int i = 0; i < velocities.Length; i++) {
                    velocities[i] /= maxVelocity.magnitude;
                }
            }

            // (float angle, float speed)[] output = new (float angle, float speed)[velocities.Length];
            for (int i = 0; i < velocities.Length; i++) {
                float speed                         = velocities[i].magnitude;
                var yComponent                      = Vector3.Dot(robotTransform.forward, velocities[i]);
                var xComponent                      = Vector3.Dot(robotTransform.right, velocities[i]);
                float angle                         = Mathf.Atan2(xComponent, yComponent) * Mathf.Rad2Deg;
                _moduleDrivers[i].azimuth.MainInput = angle;
                _moduleDrivers[i].drive.MainInput   = speed;
            }
        }

        public override void OnRemove() {
            for (int i = 0; i < _moduleDrivers.Length; i++) {
                _moduleDrivers[i].azimuth.ControlMode = RotationalDriver.RotationalControlMode.Velocity;
                _moduleDrivers[i].azimuth.MainInput   = 0f;
                _moduleDrivers[i].azimuth.Unreserve();
            }
        }

        protected override void OnDisable() {
            _moduleDrivers.ForEach(x => x.drive.MainInput = x.azimuth.MainInput = 0f);
        }
    }
}
