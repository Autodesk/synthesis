using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

using Logger   = SynthesisAPI.Utilities.Logger;
using LogLevel = SynthesisAPI.Utilities.LogLevel;
using Math     = System.Math;

#nullable enable

namespace Synthesis {
    public class MecanumDriveBehaviour : SimBehaviour {
        internal const string FORWARD             = "Mecanum Forward";
        internal const string BACKWARD            = "Mecanum Backward";
        internal const string LEFT                = "Mecanum Left";
        internal const string RIGHT               = "Mecanum Right";
        internal const string TURN_LEFT           = "Mecanum Turn Left";
        internal const string TURN_RIGHT          = "Mecanum Turn Right";
        internal const string RESET_FIELD_FORWARD = "Mecanum Reset Forward";

        private readonly string forward             = FORWARD;
        private readonly string backward            = BACKWARD;
        private readonly string left                = LEFT;
        private readonly string right               = RIGHT;
        private readonly string turn_left           = TURN_LEFT;
        private readonly string turn_right          = TURN_RIGHT;
        private readonly string reset_field_forward = RESET_FIELD_FORWARD;

        private List<WheelDriver> _frontLeftWheels;
        private List<WheelDriver> _frontRightWheels;
        private List<WheelDriver> _backRightWheels;
        private List<WheelDriver> _backLeftWheels;
        private readonly RobotSimObject _robot;

        private Vector3 _fieldForward;

        /// <summary>
        /// Wheels must be in this quadrant order: I (Front Right), II (Front Left), III (Back Left), IV (Back Right)
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="wheels"></param>
        public MecanumDriveBehaviour(RobotSimObject robot, List<WheelDriver> frontLeft, List<WheelDriver> frontRight,
            List<WheelDriver> backRight, List<WheelDriver> backLeft)
            : base(robot.Name, false) {
            _frontLeftWheels  = frontLeft;
            _frontRightWheels = frontRight;
            _backRightWheels  = backRight;
            _backLeftWheels   = backLeft;

            _robot        = robot;
            _fieldForward = Vector3.forward;

            forward             = _robot.RobotGUID + "Mecanum Forward";
            backward            = _robot.RobotGUID + "Mecanum Backward";
            left                = _robot.RobotGUID + "Mecanum Left";
            right               = _robot.RobotGUID + "Mecanum Right";
            turn_left           = _robot.RobotGUID + "Mecanum Turn Left";
            turn_right          = _robot.RobotGUID + "Mecanum Turn Right";
            reset_field_forward = _robot.RobotGUID + "Mecanum Reset Forward";

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
                (reset_field_forward, RESET_FIELD_FORWARD, TryLoadInput(reset_field_forward, new Digital("R"))) };
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

            Vector3 headingVector = _robot.GroundedNode.transform.forward -
                                    (Vector3.up * Vector3.Dot(Vector3.up, _robot.GroundedNode.transform.forward));
            float headingVectorY = Vector3.Dot(_fieldForward, headingVector);
            float headingVectorX = Vector3.Dot(Vector3.Cross(_fieldForward, Vector3.up), headingVector);
            float chassisAngle   = Mathf.Atan2(headingVectorX, headingVectorY) * Mathf.Rad2Deg;

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

            var vec     = Quaternion.AngleAxis(chassisAngle, Vector3.up) * new Vector3(moveStrafe, 0f, moveForward);
            moveForward = vec.z;
            moveStrafe  = vec.x;

            // Are the inputs basically zero
            if (moveForward == 0f && moveTurn == 0f && moveStrafe == 0f) {
                _frontLeftWheels.ForEach(x => x.MainInput = 0f);
                _frontRightWheels.ForEach(x => x.MainInput = 0f);
                _backRightWheels.ForEach(x => x.MainInput = 0f);
                _backLeftWheels.ForEach(x => x.MainInput = 0f);
                return;
            }

            var frontLeftSpeed  = moveForward + moveStrafe - moveTurn;
            var frontRightSpeed = moveForward - moveStrafe + moveTurn;
            var backLeftSpeed   = moveForward - moveStrafe - moveTurn;
            var backRightSpeed  = moveForward + moveStrafe + moveTurn;

            _frontLeftWheels.ForEach(x => x.MainInput = frontLeftSpeed);
            _frontRightWheels.ForEach(x => x.MainInput = frontRightSpeed);
            _backRightWheels.ForEach(x => x.MainInput = backRightSpeed);
            _backLeftWheels.ForEach(x => x.MainInput = backLeftSpeed);
        }

        // public override void OnRemove() { }

        protected override void OnEnable() {
            Vector3 northWest = new Vector3(-1, 0, 1).normalized;
            Vector3 northEast = new Vector3(1, 0, 1).normalized;

            _frontLeftWheels.ForEach(x => x.LocalRoller = northWest);
            _frontRightWheels.ForEach(x => x.LocalRoller = northEast);
            _backRightWheels.ForEach(x => x.LocalRoller = northWest);
            _backLeftWheels.ForEach(x => x.LocalRoller = northEast);
        }

        protected override void OnDisable() {
            _frontLeftWheels.ForEach(x => {
                x.MainInput = 0f;
                x.MatchRollerToWheelType();
            });
            _frontRightWheels.ForEach(x => {
                x.MainInput = 0f;
                x.MatchRollerToWheelType();
            });
            _backRightWheels.ForEach(x => {
                x.MainInput = 0f;
                x.MatchRollerToWheelType();
            });
            _backLeftWheels.ForEach(x => {
                x.MainInput = 0f;
                x.MatchRollerToWheelType();
            });
        }
    }
}
