using Newtonsoft.Json;
using Synthesis.Field;
using Synthesis.Input.Enums;
using Synthesis.Input.Inputs;
using System;
using System.Collections.Generic;

//=========================================================================================
//                                      Profile Class
// Description: Manages the control mappings for player profiles
//=========================================================================================

namespace Synthesis.Input
{
    public class Profile
    {
        public enum Mode// Order must correspond to that in the dropdown list within Unity
        {
            // ArcadeJoystick,
            ArcadeKeyboard,
            // MecanumJoystick,
            // MecanumKeyboard,
            TankJoystick,
            // TankKeyboard
            // Custom1,
            // Custom2,
            // Custom3
        }

        /// <summary>
        /// <see cref="Buttons"/> is a set of user defined buttons.
        /// </summary>
        public class Buttons
        {
            //Basic robot controls
            [JsonProperty]
            public KeyMapping forward;
            [JsonProperty]
            public KeyMapping backward;
            [JsonProperty]
            public KeyMapping left;
            [JsonProperty]
            public KeyMapping right;

            //Tank drive controls
            [JsonProperty]
            public KeyMapping tankFrontLeft;
            [JsonProperty]
            public KeyMapping tankBackLeft;
            [JsonProperty]
            public KeyMapping tankFrontRight;
            [JsonProperty]
            public KeyMapping tankBackRight;

            //Remaining PWM Controls
            [JsonProperty]
            public KeyMapping[] pwmPos;
            [JsonProperty]
            public KeyMapping[] pwmNeg;

            //Other controls
            [JsonProperty]
            public KeyMapping resetRobot;
            [JsonProperty]
            public KeyMapping resetField;
            [JsonProperty]
            public KeyMapping cameraToggle;
            [JsonProperty]
            public KeyMapping scoreboard;
            [JsonProperty]
            public KeyMapping trajectory;
            [JsonProperty]
            public KeyMapping replayMode;
            [JsonProperty]
            public KeyMapping duplicateRobot;
            [JsonProperty]
            public KeyMapping switchActiveRobot;

            //driver practice controls
            [JsonProperty]
            public List<KeyMapping> pickup;
            [JsonProperty]
            public List<KeyMapping> release;
            [JsonProperty]
            public List<KeyMapping> spawnPieces;

            public Buttons()
            {
                pwmPos = new KeyMapping[DriveJoints.PWM_COUNT - DriveJoints.PWM_OFFSET];
                pwmNeg = new KeyMapping[DriveJoints.PWM_COUNT - DriveJoints.PWM_OFFSET];

                pickup = new List<KeyMapping>();
                release = new List<KeyMapping>();
                spawnPieces = new List<KeyMapping>();

                // Leave fields null
            }
        }

        /// <summary>
        /// <see cref="Axes"/> is a set of user defined axes.
        /// </summary>
        public class Axes
        {
            //Arcade Axes
            public Axis vertical;
            public Axis horizontal;

            //Tank Axes
            public Axis tankLeftAxes;
            public Axis tankRightAxes;

            //PWM Axes

            public Axis[] pwmAxes;

            public Axes()
            {
                pwmAxes = new Axis[DriveJoints.PWM_COUNT - DriveJoints.PWM_OFFSET];
                // Leave fields null
            }
        }

        /// <summary>
        /// Set of buttons.
        /// </summary>
        [JsonProperty]
        public Buttons buttons;

        /// <summary>
        /// Set of axes.
        /// </summary>
        [JsonProperty]
        public Axes axes;

        public Profile()
        {
            buttons = new Buttons();
            axes = new Axes();
        }

        public new string ToString()
        {
            return JsonConvert.SerializeObject(buttons, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        public void FromString(string input, bool init = false)
        {
            if (init)
            {
                buttons = JsonConvert.DeserializeObject<Buttons>(input, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
            }
            else
            {
                JsonConvert.PopulateObject(input, buttons, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
            }
        }

        public static Profile CreateDefault(int joystick_index, Mode controlProfile)
        {
            Profile profile = new Profile();

            if (Enum.TryParse("Joystick" + joystick_index, false, out Joystick joy))
            {
                switch (controlProfile)
                {
                    case Mode.ArcadeKeyboard:
                        {
                            #region Arcade Controls

                            profile.buttons.forward = new KeyMapping("Forward", new JoystickInput(JoystickAxis.Axis2Negative, joy));
                            profile.buttons.backward = new KeyMapping("Backward", new JoystickInput(JoystickAxis.Axis2Positive, joy));
                            profile.buttons.left = new KeyMapping("Left", new JoystickInput(JoystickAxis.Axis4Negative, joy));
                            profile.buttons.right = new KeyMapping("Right", new JoystickInput(JoystickAxis.Axis4Positive, joy));

                            profile.axes.horizontal = new Axis("Arcade Horizontal", profile.buttons.left, profile.buttons.right);
                            profile.axes.vertical = new Axis("Arcade Vertical", profile.buttons.backward, profile.buttons.forward);

                            #endregion
                        }
                        break;
                    case Mode.TankJoystick:
                        {
                            #region Tank Controls

                            profile.buttons.tankFrontLeft = new KeyMapping("Tank Front Left", new JoystickInput(JoystickAxis.Axis2Negative, joy));
                            profile.buttons.tankBackLeft = new KeyMapping("Tank Back Left", new JoystickInput(JoystickAxis.Axis2Positive, joy));
                            profile.buttons.tankFrontRight = new KeyMapping("Tank Front Right", new JoystickInput(JoystickAxis.Axis5Positive, joy));
                            profile.buttons.tankBackRight = new KeyMapping("Tank Back Right", new JoystickInput(JoystickAxis.Axis5Negative, joy));

                            profile.axes.tankLeftAxes = new Axis("Tank Left", profile.buttons.tankBackLeft, profile.buttons.tankFrontLeft);
                            profile.axes.tankRightAxes = new Axis("Tank Right", profile.buttons.tankBackRight, profile.buttons.tankFrontRight);

                            #endregion
                        }
                        break;
                    default:
                        throw new UnhandledControlProfileException();
                }


                #region General Controls

                //PWM controls
                for (int pwm_i = 0; pwm_i < DriveJoints.PWM_COUNT - DriveJoints.PWM_OFFSET; pwm_i++)
                {
                    var pwm_key_i = pwm_i + 1;
                    if (Enum.TryParse("Axis" + pwm_key_i + "Positive", false, out JoystickAxis axis_pos) &&
                        Enum.TryParse("Axis" + pwm_key_i + "Negative", false, out JoystickAxis axis_neg))
                    {
                        profile.buttons.pwmPos[pwm_i] = new KeyMapping("PWM " + (pwm_i + DriveJoints.PWM_OFFSET) + " Positive", new JoystickInput(axis_pos, joy));
                        profile.buttons.pwmNeg[pwm_i] = new KeyMapping("PWM " + (pwm_i + DriveJoints.PWM_OFFSET) + " Negative", new JoystickInput(axis_neg, joy));

                        profile.axes.pwmAxes[pwm_i] = new Axis("PWM " + (pwm_i + DriveJoints.PWM_OFFSET), profile.buttons.pwmNeg[pwm_i], profile.buttons.pwmPos[pwm_i]);
                    }
                    else
                    {
                        throw new Exception("Error configuring PWM buttons");
                    }
                }

                //Other Controls
                profile.buttons.resetRobot = new KeyMapping("Reset Robot", new JoystickInput(JoystickButton.Button8, joy));
                profile.buttons.resetField = new KeyMapping("Reset Field", new JoystickInput(JoystickButton.Button9, joy));
                profile.buttons.replayMode = new KeyMapping("Replay Mode", new JoystickInput(JoystickButton.Button12, joy));
                profile.buttons.cameraToggle = new KeyMapping("Camera Toggle", new JoystickInput(JoystickButton.Button7, joy));
                profile.buttons.scoreboard = new KeyMapping("Scoreboard", new JoystickInput(JoystickButton.Button10, joy));
                profile.buttons.trajectory = new KeyMapping("Toggle Trajectory", new JoystickInput(JoystickButton.Button11, joy));
                profile.buttons.duplicateRobot = new KeyMapping("Duplicate Robot", new JoystickInput(JoystickButton.Button1, joy));
                profile.buttons.switchActiveRobot = new KeyMapping("Switch Active Robot", new JoystickInput(JoystickButton.Button2, joy));

                #endregion
            }
            else
            {
                throw new Exception("Failed to establish joystick index");
            }
            profile.UpdateFieldControls(joystick_index);
            return profile;
        }

        public void UpdateFieldControls(int joystick_index)
        {
            #region Field Controls

            buttons.pickup.Clear();
            buttons.release.Clear();
            buttons.spawnPieces.Clear();

            for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
            {
                if (Enum.TryParse("Joystick" + joystick_index, false, out Joystick joy))
                {
                    buttons.pickup.Add(new KeyMapping("Pick Up " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button3, joy)));
                    buttons.release.Add(new KeyMapping("Release " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button4, joy)));
                    buttons.spawnPieces.Add(new KeyMapping("Spawn " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button5, joy)));
                }
                else
                {
                    throw new Exception("Failed to establish joystick index");
                }
            }

            #endregion
        }

        public class UnhandledControlProfileException : Exception
        {
            public UnhandledControlProfileException() { }

            public UnhandledControlProfileException(string message) : base(message) { }

            public UnhandledControlProfileException(string message, Exception inner) : base(message, inner) { }
        }

    }
}
