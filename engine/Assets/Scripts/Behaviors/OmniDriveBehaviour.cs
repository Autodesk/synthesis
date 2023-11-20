using System.Collections.Generic;
using Synthesis.PreferenceManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using UnityEngine;
using Logger   = SynthesisAPI.Utilities.Logger;
using LogLevel = SynthesisAPI.Utilities.LogLevel;
using Math     = System.Math;

#nullable enable

namespace Synthesis {
    public class OmniDriveBehaviour : SimBehaviour {
        internal const string FORWARD              = "Omni Forward";
        internal const string BACKWARD             = "Omni Backward";
        internal const string LEFT                 = "Omni Left";
        internal const string RIGHT                = "Omni Right";
        internal const string TURN_LEFT            = "Omni Turn Left";
        internal const string TURN_RIGHT           = "Omni Turn Right";
        internal const string RESET_FIELD_FORWARD  = "Omni Reset Forward";
        internal const string TOGGLE_FIELD_CENTRIC = "Toggle Field Centric";

        private readonly string forward              = FORWARD;
        private readonly string backward             = BACKWARD;
        private readonly string left                 = LEFT;
        private readonly string right                = RIGHT;
        private readonly string turn_left            = TURN_LEFT;
        private readonly string turn_right           = TURN_RIGHT;
        private readonly string reset_field_forward  = RESET_FIELD_FORWARD;
        private readonly string toggle_field_centric = TOGGLE_FIELD_CENTRIC;

        private List<WheelDriver> _wheels;
        private readonly RobotSimObject _robot;

        private Vector3 _fieldForward;
        private bool _fieldCentric = true;

        private float _turnFavor = 1.5f;

        /// <summary>
        /// Create a mecanum drivetrain.
        /// </summary>
        /// <param name="robot">Owning SimObject</param>
        /// <param name="frontLeft">Front-Left wheels</param>
        /// <param name="frontRight">Front-Right wheels</param>
        /// <param name="backRight">Back-Right wheels</param>
        /// <param name="backLeft">Back-Left wheels</param>
        public OmniDriveBehaviour(RobotSimObject robot, List<WheelDriver> wheels) : base(robot.Name, false) {
            _wheels = wheels;

            _robot        = robot;
            _fieldForward = Vector3.forward;

            forward              = _robot.RobotGUID + "Omni Forward";
            backward             = _robot.RobotGUID + "Omni Backward";
            left                 = _robot.RobotGUID + "Omni Left";
            right                = _robot.RobotGUID + "Omni Right";
            turn_left            = _robot.RobotGUID + "Omni Turn Left";
            turn_right           = _robot.RobotGUID + "Omni Turn Right";
            reset_field_forward  = _robot.RobotGUID + "Omni Reset Forward";
            toggle_field_centric = _robot.RobotGUID + "Toggle Field Centric";

            InitInputs(GetInputs());
            EventBus.NewTypeListener<ValueInputAssignedEvent>(OnValueInputAssigned);

            Enabled = true;
        }

        public (string key, string name, Analog input)[] GetInputs() {
            return new(
                string key, string name, Analog input)[] { (forward, FORWARD, TryLoadInput(forward, new Digital("W"))),
                (backward, BACKWARD, TryLoadInput(backward, new Digital("S"))),
                (left, LEFT, TryLoadInput(left, new Digital("A"))),
                (right, RIGHT, TryLoadInput(right, new Digital("D"))),
                (turn_left, TURN_LEFT, TryLoadInput(turn_left, new Digital("LeftArrow"))),
                (turn_right, TURN_RIGHT, TryLoadInput(turn_right, new Digital("RightArrow"))),
                (reset_field_forward, RESET_FIELD_FORWARD, TryLoadInput(reset_field_forward, new Digital("R"))),
                (toggle_field_centric, TOGGLE_FIELD_CENTRIC, TryLoadInput(toggle_field_centric, new Digital("T"))) };
        }

        public Analog TryLoadInput(string key, Analog defaultInput) {
            Analog input;
            if (InputManager.MappedValueInputs.ContainsKey(key)) {
                input                = InputManager.GetAnalog(key);
                input.ContextBitmask = defaultInput.ContextBitmask;
                return input;
            }
            input = SimulationPreferences.GetRobotInput(_robot.RobotGUID, key);
            if (input == null) {
                SimulationPreferences.SetRobotInput(_robot.RobotGUID, key, defaultInput);
                return defaultInput;
            }
            return input;
        }

        private void OnValueInputAssigned(IEvent tmp) {
            ValueInputAssignedEvent args = (tmp as ValueInputAssignedEvent)!;

            if (args.InputKey.Length > _robot.RobotGUID.Length) {
                string s = args.InputKey.Remove(0, _robot.RobotGUID.Length);
                switch (s) {
                    case FORWARD:
                    case BACKWARD:
                    case LEFT:
                    case RIGHT:
                    case TURN_LEFT:
                    case TURN_RIGHT:
                    case RESET_FIELD_FORWARD:
                    case TOGGLE_FIELD_CENTRIC:
                        SimulationPreferences.SetRobotInput(_robot.RobotGUID, args.InputKey, args.Input);
                        break;
                }
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
            if (Mathf.Abs(InputManager.MappedValueInputs[reset_field_forward].Value) > 0.5f)
                _fieldForward = _robot.GroundedNode.transform.forward;

            if (Mathf.Abs(InputManager.MappedValueInputs[toggle_field_centric].Value) > 0.5f)
                _fieldCentric = !_fieldCentric;

            var forwardInput   = InputManager.MappedValueInputs[forward];
            var backwardInput  = InputManager.MappedValueInputs[backward];
            var leftInput      = InputManager.MappedValueInputs[left];
            var rightInput     = InputManager.MappedValueInputs[right];
            var turnLeftInput  = InputManager.MappedValueInputs[turn_left];
            var turnRightInput = InputManager.MappedValueInputs[turn_right];

            float moveForward = Mathf.Abs(forwardInput.Value) - Mathf.Abs(backwardInput.Value);
            float moveStrafe  = Mathf.Abs(rightInput.Value) - Mathf.Abs(leftInput.Value);
            float moveTurn    = Mathf.Abs(turnLeftInput.Value) - Mathf.Abs(turnRightInput.Value);

            moveForward = Diff(moveForward, 0f, 0.1f) ? 0f : moveForward;
            moveStrafe  = Diff(moveStrafe, 0f, 0.1f) ? 0f : moveStrafe;
            moveTurn    = Diff(moveTurn, 0f, 0.1f) ? 0f : moveTurn;

            // Are the inputs basically zero
            if (moveForward == 0f && moveTurn == 0f && moveStrafe == 0f) {
                _wheels.ForEach(x => x.MainInput = 0f);
                return;
            }

            // Adjusts how much turning verse translation is favored
            moveTurn *= _turnFavor;

            var robotTransform = _robot.GroundedNode.transform;

            Vector3 chassisVelocity        = robotTransform.forward * moveForward + robotTransform.right * moveStrafe;
            Vector3 chassisAngularVelocity = robotTransform.up * moveTurn;

            if (chassisVelocity.magnitude > 1)
                chassisVelocity = chassisVelocity.normalized;

            // Rotate chassis velocity by chassis angle
            if (_fieldCentric) {
                Vector3 headingVector = _robot.GroundedNode.transform.forward -
                                        (Vector3.up * Vector3.Dot(Vector3.up, _robot.GroundedNode.transform.forward));
                float headingVectorY = Vector3.Dot(_fieldForward, headingVector);
                float headingVectorX = Vector3.Dot(Vector3.Cross(_fieldForward, Vector3.up), headingVector);
                float chassisAngle   = Mathf.Atan2(headingVectorX, headingVectorY) * Mathf.Rad2Deg;
                chassisVelocity      = Quaternion.AngleAxis(chassisAngle, robotTransform.up) * chassisVelocity;
            }

            var maxVelocity    = 1.0f;
            float[] velocities = new float[_wheels.Count];
            for (int i = 0; i < velocities.Length; i++) {
                var wheel = _wheels[i];
                var com   = robotTransform.localToWorldMatrix.MultiplyPoint3x4(
                    robotTransform.GetComponent<Rigidbody>().centerOfMass);
                var radius = wheel.Anchor - com;

                // Remove axis component of radius
                radius -= Vector3.Dot(robotTransform.up, radius) * robotTransform.up;

                velocities[i] = Vector3.Dot(Vector3.Cross(wheel.Axis, robotTransform.up).normalized,
                    Vector3.Cross(chassisAngularVelocity, radius) + chassisVelocity);
                if (Mathf.Abs(velocities[i]) > maxVelocity)
                    maxVelocity = Mathf.Abs(velocities[i]);
            }

            for (int i = 0; i < velocities.Length; i++) {
                _wheels[i].MainInput = velocities[i] / maxVelocity;
            }
        }

        protected override void OnEnable() {
            bool wheelTypeWarning = false;
            _wheels.ForEach(x => {
                if (x.LocalRoller == null) {
                    wheelTypeWarning = true;
                    x.LocalRoller    = Vector3.right;
                }
            });

            if (wheelTypeWarning) {
                Logger.Log("Switching standard wheels to omni while using omni drive", LogLevel.Warning);
            }
        }

        protected override void OnDisable() {
            _wheels.ForEach(x => {
                x.MainInput = 0.0f;
                x.MatchRollerToWheelType();
            });
        }
    }
}
