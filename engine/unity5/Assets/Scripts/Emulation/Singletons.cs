using EmulationService;

namespace Synthesis
{

    public static class OutputManager
    {

        static OutputManager() { }
        public static RobotOutputs Instance { get { return OutputInternal.instance; } set { OutputInternal.instance = value; } }

        public const uint NUM_PWM_HDRS = 10;
        public const uint NUM_PWM_MXP  = 10;
        public const uint NUM_DIO_HDRS = 10;
        public const uint NUM_DIO_MXP  = 10;
        public const uint NUM_AO_MXP   = 2;

        private class OutputInternal
        {
            static OutputInternal() { }
            internal static RobotOutputs instance = new RobotOutputs();
        }
    }

    public static class InputManager
    {
        public static RobotInputs Instance { get { return InputInternal.instance; } set { InputInternal.instance = value; } }

        public const uint NUM_AI_HDRS = 4;
        public const uint NUM_AI_MXP  = 4;

        public const uint NUM_JOYSTICKS = 6;
        public const uint NUM_DIGITAL_HEADERS = 10;
        public const uint NUM_DIGITAL_MXP = 16;
        public const uint NUM_ENCODERS = 8;

        public const uint NUM_JOYSTICK_AXES = 12;
        public const uint NUM_JOYSTICK_BUTTONS = 10;
        public const uint NUM_JOYSTICK_POVS = 12;

        private class InputInternal
        {
            static InputInternal() { }
            internal static RobotInputs instance = InitRobotInputs();

            private static RobotInputs InitRobotInputs()
            {
                RobotInputs inputs = new RobotInputs();
                for (var i = 0; i < NUM_DIGITAL_MXP; i++)
                    inputs.MxpData.Add(InitMxpData());
                for (var i = 0; i < NUM_DIGITAL_HEADERS; i++)
                    inputs.DigitalHeaders.Add(false);
                inputs.RobotMode = InitRobotMode();
                inputs.MatchInfo = InitMatchInfo();
                for (var i = 0; i < NUM_JOYSTICKS; i++)
                    inputs.Joysticks.Add(InitJoystick());
                // Let DriveJoints handle EncoderManagers
                return inputs;
            }

            private static MXPData InitMxpData()
            {
                return new MXPData
                {
                    Value = 0,
                    MxpConfig = MXPData.Types.MXPConfig.Di
                };
            }

            private static RobotInputs.Types.Joystick InitJoystick()
            {
                var joystick = new RobotInputs.Types.Joystick();
                joystick.Outputs = 0;
                joystick.PovCount = (int)NUM_JOYSTICK_POVS;
                for (var i = 0; i < joystick.PovCount; i++)
                    joystick.Povs.Add(-1);
                joystick.AxisCount = (int)NUM_JOYSTICK_AXES;
                for (var i = 0; i < joystick.AxisCount; i++)
                    joystick.AxisTypes.Add(0);
                for (var i = 0; i < joystick.AxisCount; i++)
                    joystick.Axes.Add(0);
                joystick.ButtonCount = (int)NUM_JOYSTICK_BUTTONS;
                joystick.Buttons = 0;
                joystick.Name = "";
                joystick.Type = 0;
                joystick.RightRumble = 0;
                joystick.LeftRumble = 0;
                joystick.IsXbox = false;
                return joystick;
            }

            private static RobotInputs.Types.RobotMode InitRobotMode()
            {
                return new RobotInputs.Types.RobotMode
                {
                    Enabled = false,
                    IsDsAttached = true,
                    IsFmsAttached = true,
                    IsEmergencyStopped = false,
                    Mode = RobotInputs.Types.RobotMode.Types.Mode.Teleop
                };
            }

            private static RobotInputs.Types.MatchInfo InitMatchInfo()
            {
                return new RobotInputs.Types.MatchInfo
                {
                    EventName = "Synthesis Simulation",
                    MatchType = RobotInputs.Types.MatchInfo.Types.MatchType.None,
                    GameSpecificMessage = "",
                    ReplayNumber = 0,
                    MatchTime = 0, // TODO?
                    MatchNumber = 0,
                    AllianceStationId = RobotInputs.Types.MatchInfo.Types.AllianceStationID.Red1
                };
            }
        }
    }
}