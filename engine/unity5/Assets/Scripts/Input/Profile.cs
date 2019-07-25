using Newtonsoft.Json;
using Synthesis.Field;
using Synthesis.Input.Enums;
using Synthesis.Input.Inputs;
using System;
using System.Collections.Generic;
using UnityEngine;

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
            Arcade,
            Mecanum,
            Tank,
            // Custom1,
            // Custom2,
            // Custom3
        }

        /// <summary>
        /// <see cref="Buttons"/> is a set of user defined buttons.
        /// </summary>
        public class Buttons
        {
            //PWM Controls
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

            //Driver practice controls
            [JsonProperty]
            public List<KeyMapping> pickup;
            [JsonProperty]
            public List<KeyMapping> release;
            [JsonProperty]
            public List<KeyMapping> spawnPieces;

            public Buttons()
            {
                pwmPos = new KeyMapping[DriveJoints.PWM_COUNT];
                pwmNeg = new KeyMapping[DriveJoints.PWM_COUNT];

                pickup = new List<KeyMapping>();
                release = new List<KeyMapping>();
                spawnPieces = new List<KeyMapping>();

                // Leave fields null
            }

            public List<KeyMapping> ToList()
            {
                // Keep this up-to-date with all this class's fields
                // The order they appear here is the order they'll appear in the controls menu

                List<KeyMapping> list = new List<KeyMapping>();
                for(int i = 0; i < DriveJoints.PWM_COUNT; i++)
                {
                    list.Add(pwmPos[i]);
                    list.Add(pwmNeg[i]);
                }

                list.Add(resetRobot);
                list.Add(resetField);
                list.Add(cameraToggle);
                list.Add(scoreboard);
                list.Add(trajectory);
                list.Add(replayMode);
                list.Add(duplicateRobot);
                list.Add(switchActiveRobot);

                foreach (var mapping in pickup)
                    list.Add(mapping);
                foreach (var mapping in release)
                    list.Add(mapping);
                foreach (var mapping in spawnPieces)
                    list.Add(mapping);

                return list;
            }
        }

        /// <summary>
        /// <see cref="Axes"/> is a set of user defined axes.
        /// </summary>
        public class Axes
        {
            //PWM Axes

            public Axis[] pwmAxes;

            public Axes()
            {
                pwmAxes = new Axis[DriveJoints.PWM_COUNT];
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

        public static Profile CreateDefault(int player_i, Mode controlProfile)
        {
            Profile profile = new Profile();

            var joystick_index = player_i + 1;

            if (Enum.TryParse("Joystick" + joystick_index, false, out Joystick joy))
            {
                #region Default Controls

                #region PWM Controls

                //PWM controls
                for (int pwm_i = 0; pwm_i < DriveJoints.PWM_COUNT; pwm_i++)
                {
                    if (player_i == 0) // Customized keyboard player 1 controls
                    {
                        if (Enum.TryParse("Alpha" + pwm_i, false, out KeyCode val))
                        {
                            profile.buttons.pwmPos[pwm_i] = new KeyMapping("PWM " + pwm_i + " Positive", val);
                            profile.buttons.pwmNeg[pwm_i] = new KeyMapping("PWM " + pwm_i + " Negative", val, KeyModifier.Shift);
                        }
                        else
                        {
                            throw new Exception("Error configuring PWM buttons");
                        }
                    }
                    else
                    {
                        var pwm_key_i = pwm_i + 1;
                        if (Enum.TryParse("Axis" + pwm_key_i + "Positive", false, out JoystickAxis axis_pos) &&
                        Enum.TryParse("Axis" + pwm_key_i + "Negative", false, out JoystickAxis axis_neg))
                        {
                            profile.buttons.pwmPos[pwm_i] = new KeyMapping("PWM " + pwm_i + " Positive", new JoystickInput(axis_pos, joy));
                            profile.buttons.pwmNeg[pwm_i] = new KeyMapping("PWM " + pwm_i + " Negative", new JoystickInput(axis_neg, joy));
                        }
                        else
                        {
                            throw new Exception("Error configuring PWM buttons");
                        }
                    }

                    profile.axes.pwmAxes[pwm_i] = new Axis("PWM " + pwm_i, profile.buttons.pwmNeg[pwm_i], profile.buttons.pwmPos[pwm_i]);
                }

                // Default PWM addresses for various motors for use in default profiles
                const int FRONT_LEFT_PWM = 0;
                const int FRONT_RIGHT_PWM = 1;
                const int BACK_LEFT_PWM = 2;
                const int BACK_RIGHT_PWM = 3;

                switch (controlProfile)
                {
                    case Mode.Arcade:
                        {
                            #region Arcade Controls

                            if (player_i == 0) // Customized keyboard player 1 controls
                            {
                                profile.buttons.pwmPos[FRONT_LEFT_PWM].set(KeyCode.DownArrow, KeyCode.RightArrow);
                                profile.buttons.pwmNeg[FRONT_LEFT_PWM].set(KeyCode.UpArrow, KeyCode.LeftArrow);
                                profile.buttons.pwmPos[FRONT_RIGHT_PWM].set(KeyCode.UpArrow, KeyCode.RightArrow);
                                profile.buttons.pwmNeg[FRONT_RIGHT_PWM].set(KeyCode.DownArrow, KeyCode.LeftArrow);
                            }
                            else
                            {
                                profile.buttons.pwmPos[FRONT_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Positive, joy), new JoystickInput(JoystickAxis.Axis1Positive, joy));
                                profile.buttons.pwmNeg[FRONT_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Negative, joy), new JoystickInput(JoystickAxis.Axis1Negative, joy));
                                profile.buttons.pwmPos[FRONT_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis2Negative, joy), new JoystickInput(JoystickAxis.Axis1Positive, joy));
                                profile.buttons.pwmNeg[FRONT_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis2Positive, joy), new JoystickInput(JoystickAxis.Axis1Negative, joy));
                            }

                            #endregion
                        }
                        break;
                    case Mode.Mecanum:
                        {
                            #region Mecanum Controls

                            if (player_i == 0) // Customized keyboard player 1 controls
                            {
                                profile.buttons.pwmPos[FRONT_LEFT_PWM].set(KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.Alpha2);
                                profile.buttons.pwmNeg[FRONT_LEFT_PWM].set(KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.Alpha1);
                                profile.buttons.pwmPos[BACK_LEFT_PWM].set(KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.Alpha1);
                                profile.buttons.pwmNeg[BACK_LEFT_PWM].set(KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.Alpha2);

                                profile.buttons.pwmPos[FRONT_RIGHT_PWM].set(KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.Alpha2);
                                profile.buttons.pwmNeg[FRONT_RIGHT_PWM].set(KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.Alpha1);
                                profile.buttons.pwmPos[BACK_RIGHT_PWM].set(KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.Alpha1);
                                profile.buttons.pwmNeg[BACK_RIGHT_PWM].set(KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.Alpha2);
                            }
                            else
                            {
                                profile.buttons.pwmPos[FRONT_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Positive, joy), new JoystickInput(JoystickAxis.Axis4Positive, joy), new JoystickInput(JoystickAxis.Axis1Positive, joy));
                                profile.buttons.pwmNeg[FRONT_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Negative, joy), new JoystickInput(JoystickAxis.Axis4Negative, joy), new JoystickInput(JoystickAxis.Axis1Negative, joy));
                                profile.buttons.pwmPos[BACK_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Positive, joy), new JoystickInput(JoystickAxis.Axis4Positive, joy), new JoystickInput(JoystickAxis.Axis1Negative, joy));
                                profile.buttons.pwmNeg[BACK_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Negative, joy), new JoystickInput(JoystickAxis.Axis4Negative, joy), new JoystickInput(JoystickAxis.Axis1Positive, joy));

                                profile.buttons.pwmPos[FRONT_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis2Negative, joy), new JoystickInput(JoystickAxis.Axis4Positive, joy), new JoystickInput(JoystickAxis.Axis1Positive, joy));
                                profile.buttons.pwmNeg[FRONT_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis2Positive, joy), new JoystickInput(JoystickAxis.Axis4Negative, joy), new JoystickInput(JoystickAxis.Axis1Negative, joy));
                                profile.buttons.pwmPos[BACK_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis2Negative, joy), new JoystickInput(JoystickAxis.Axis4Positive, joy), new JoystickInput(JoystickAxis.Axis1Negative, joy));
                                profile.buttons.pwmNeg[BACK_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis2Positive, joy), new JoystickInput(JoystickAxis.Axis4Negative, joy), new JoystickInput(JoystickAxis.Axis1Positive, joy));
                            }

                            #endregion
                        }
                        break;
                    case Mode.Tank:
                        {
                            #region Tank Controls

                            // Player 1's tank controls are the same as the other players'
                            profile.buttons.pwmPos[FRONT_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Positive, joy));
                            profile.buttons.pwmNeg[FRONT_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Negative, joy));
                            profile.buttons.pwmPos[FRONT_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis5Negative, joy));
                            profile.buttons.pwmNeg[FRONT_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis5Positive, joy));

                            #endregion
                        }
                        break;
                    default:
                        throw new UnhandledControlProfileException();
                }

                #endregion

                #region Other Controls
                //Other Controls
                if (player_i == 0) // Customized keyboard player 1 controls
                {
                    profile.buttons.resetRobot = new KeyMapping("Reset Robot", KeyCode.R);
                    profile.buttons.resetField = new KeyMapping("Reset Field", KeyCode.F);
                    profile.buttons.replayMode = new KeyMapping("Replay Mode", KeyCode.Tab);
                    profile.buttons.cameraToggle = new KeyMapping("Camera Toggle", KeyCode.C);
                    profile.buttons.scoreboard = new KeyMapping("Scoreboard", KeyCode.Q);
                    profile.buttons.trajectory = new KeyMapping("Toggle Trajectory", KeyCode.T);
                    profile.buttons.duplicateRobot = new KeyMapping("Duplicate Robot", KeyCode.U);
                    profile.buttons.switchActiveRobot = new KeyMapping("Switch Active Robot", KeyCode.Y);
                }
                else
                {
                    profile.buttons.resetRobot = new KeyMapping("Reset Robot", new JoystickInput(JoystickButton.Button8, joy));
                    profile.buttons.resetField = new KeyMapping("Reset Field", new JoystickInput(JoystickButton.Button9, joy));
                    profile.buttons.replayMode = new KeyMapping("Replay Mode", new JoystickInput(JoystickButton.Button12, joy));
                    profile.buttons.cameraToggle = new KeyMapping("Camera Toggle", new JoystickInput(JoystickButton.Button7, joy));
                    profile.buttons.scoreboard = new KeyMapping("Scoreboard", new JoystickInput(JoystickButton.Button10, joy));
                    profile.buttons.trajectory = new KeyMapping("Toggle Trajectory", new JoystickInput(JoystickButton.Button11, joy));
                    profile.buttons.duplicateRobot = new KeyMapping("Duplicate Robot", new JoystickInput(JoystickButton.Button1, joy));
                    profile.buttons.switchActiveRobot = new KeyMapping("Switch Active Robot", new JoystickInput(JoystickButton.Button2, joy));
                }

                #endregion

                #endregion
            }
            else
            {
                throw new Exception("Failed to establish joystick index");
            }
            profile.UpdateFieldControls(player_i);
            return profile;
        }

        public void UpdateFieldControls(int player_i)
        {
            #region Field Controls

            buttons.pickup.Clear();
            buttons.release.Clear();
            buttons.spawnPieces.Clear();

            var joystick_index = player_i + 1;

            for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
            {
                if (Enum.TryParse("Joystick" + joystick_index, false, out Joystick joy))
                {
                    if (player_i == 0)
                    {
                        buttons.pickup.Add(new KeyMapping("1: Pick Up " + FieldDataHandler.gamepieces[i].name, KeyCode.LeftControl));
                        buttons.release.Add(new KeyMapping("1: Release " + FieldDataHandler.gamepieces[i].name, KeyCode.LeftAlt));
                        buttons.spawnPieces.Add(new KeyMapping("1: Spawn " + FieldDataHandler.gamepieces[i].name, KeyCode.RightControl));

                    }
                    else
                    {
                        buttons.pickup.Add(new KeyMapping("Pick Up " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button3, joy)));
                        buttons.release.Add(new KeyMapping("Release " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button4, joy)));
                        buttons.spawnPieces.Add(new KeyMapping("Spawn " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button5, joy)));
                    }
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
