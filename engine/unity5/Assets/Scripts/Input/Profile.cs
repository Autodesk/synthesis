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
            public KeyMapping[] pwmPos; // TODO: maybe move pos and neg keymappings into Axis class, move Axes into this class, and rename to KeyMappings?
            [JsonProperty]
            public KeyMapping[] pwmNeg;

            //Other controls
            [JsonProperty]
            public KeyMapping resetRobot;
            [JsonProperty]
            public KeyMapping trajectory;
            [JsonProperty]
            public KeyMapping duplicateRobot;
            [JsonProperty]
            public KeyMapping switchActiveRobot;

            //Driver practice controls
            [JsonProperty]
            public List<KeyMapping> spawnPieces;
            [JsonProperty]
            public List<KeyMapping> pickup;
            [JsonProperty]
            public List<KeyMapping> release;

            public Buttons()
            {
                pwmPos = new KeyMapping[DriveJoints.PWM_HDR_COUNT];
                pwmNeg = new KeyMapping[DriveJoints.PWM_HDR_COUNT];

                spawnPieces = new List<KeyMapping>();
                pickup = new List<KeyMapping>();
                release = new List<KeyMapping>();

                // Leave fields null
            }

            public List<KeyMapping> ToList()
            {
                // Keep this up-to-date with all this class's fields
                // The order they appear here is the order they'll appear in the controls menu

                List<KeyMapping> list = new List<KeyMapping>();
                for(int i = 0; i < DriveJoints.PWM_HDR_COUNT; i++)
                {
                    list.Add(pwmPos[i]);
                    list.Add(pwmNeg[i]);
                }

                list.Add(resetRobot);
                list.Add(trajectory);
                list.Add(duplicateRobot);
                list.Add(switchActiveRobot);

                foreach (var mapping in spawnPieces)
                    list.Add(mapping);
                foreach (var mapping in pickup)
                    list.Add(mapping);
                foreach (var mapping in release)
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
                pwmAxes = new Axis[DriveJoints.PWM_HDR_COUNT];

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

        private static JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        public new string ToString()
        {
            return JsonConvert.SerializeObject(this, JSON_SETTINGS);
        }

        public void FromString(string input)
        {
            JsonConvert.PopulateObject(input, this, JSON_SETTINGS);
        }

        // Default PWM addresses for various motors for use in default profiles
        public const int FRONT_LEFT_PWM = 1;
        public const int FRONT_RIGHT_PWM = 0;
        public const int BACK_LEFT_PWM = 2;
        public const int BACK_RIGHT_PWM = 3;

        public void Reset(int player_i, Mode controlMode)
        {
            var joystick_index = player_i + 1;

            if (Enum.TryParse("Joystick" + joystick_index, false, out Joystick joy))
            {
                #region Default Controls

                #region PWM Controls

                //PWM controls
                for (int pwm_i = 0; pwm_i < DriveJoints.PWM_HDR_COUNT; pwm_i++)
                {
                    if (player_i == 0) // Customized keyboard player 1 controls
                    {
                        if (Enum.TryParse("Alpha" + pwm_i, false, out KeyCode val))
                        {
                            buttons.pwmPos[pwm_i] = new KeyMapping("PWM " + pwm_i + " Positive", val);
                            buttons.pwmNeg[pwm_i] = new KeyMapping("PWM " + pwm_i + " Negative", val, KeyModifier.Shift);
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
                            buttons.pwmPos[pwm_i] = new KeyMapping("PWM " + pwm_i + " Positive", new JoystickInput(axis_pos, joy));
                            buttons.pwmNeg[pwm_i] = new KeyMapping("PWM " + pwm_i + " Negative", new JoystickInput(axis_neg, joy));
                        }
                        else
                        {
                            throw new Exception("Error configuring PWM buttons");
                        }
                    }

                    axes.pwmAxes[pwm_i] = new Axis(" PWM " + pwm_i, buttons.pwmNeg[pwm_i], buttons.pwmPos[pwm_i]);
                }

                switch (controlMode)
                {
                    case Mode.Arcade:
                        {
                            #region Arcade Controls

                            if (player_i == 0) // Customized keyboard player 1 controls
                            {
                                buttons.pwmPos[FRONT_LEFT_PWM].set(KeyCode.UpArrow, KeyCode.RightArrow);
                                buttons.pwmNeg[FRONT_LEFT_PWM].set(KeyCode.DownArrow, KeyCode.LeftArrow);
                                buttons.pwmPos[FRONT_RIGHT_PWM].set(KeyCode.DownArrow, KeyCode.RightArrow);
                                buttons.pwmNeg[FRONT_RIGHT_PWM].set(KeyCode.UpArrow, KeyCode.LeftArrow);
                            }
                            else
                            {
                                buttons.pwmPos[FRONT_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Negative, joy), new JoystickInput(JoystickAxis.Axis4Positive, joy));
                                buttons.pwmNeg[FRONT_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Positive, joy), new JoystickInput(JoystickAxis.Axis4Negative, joy));
                                buttons.pwmPos[FRONT_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis2Positive, joy), new JoystickInput(JoystickAxis.Axis4Positive, joy));
                                buttons.pwmNeg[FRONT_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis2Negative, joy), new JoystickInput(JoystickAxis.Axis4Negative, joy));
                            }

                            #endregion
                        }
                        break;
                    case Mode.Mecanum:
                        {
                            #region Mecanum Controls

                            if (player_i == 0) // Customized keyboard player 1 controls
                            {
                                buttons.pwmPos[FRONT_LEFT_PWM].set(KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.Alpha2);
                                buttons.pwmNeg[FRONT_LEFT_PWM].set(KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.Alpha1);
                                buttons.pwmPos[BACK_LEFT_PWM].set(KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.Alpha1);
                                buttons.pwmNeg[BACK_LEFT_PWM].set(KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.Alpha2);

                                buttons.pwmPos[FRONT_RIGHT_PWM].set(KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.Alpha2);
                                buttons.pwmNeg[FRONT_RIGHT_PWM].set(KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.Alpha1);
                                buttons.pwmPos[BACK_RIGHT_PWM].set(KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.Alpha1);
                                buttons.pwmNeg[BACK_RIGHT_PWM].set(KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.Alpha2);
                            }
                            else
                            {
                                buttons.pwmPos[FRONT_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Negative, joy), new JoystickInput(JoystickAxis.Axis4Positive, joy), new JoystickInput(JoystickAxis.Axis1Positive, joy));
                                buttons.pwmNeg[FRONT_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Positive, joy), new JoystickInput(JoystickAxis.Axis4Negative, joy), new JoystickInput(JoystickAxis.Axis1Negative, joy));
                                buttons.pwmPos[BACK_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Positive, joy), new JoystickInput(JoystickAxis.Axis4Positive, joy), new JoystickInput(JoystickAxis.Axis1Negative, joy));
                                buttons.pwmNeg[BACK_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Negative, joy), new JoystickInput(JoystickAxis.Axis4Negative, joy), new JoystickInput(JoystickAxis.Axis1Positive, joy));

                                buttons.pwmPos[FRONT_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis2Positive, joy), new JoystickInput(JoystickAxis.Axis4Positive, joy), new JoystickInput(JoystickAxis.Axis1Positive, joy));
                                buttons.pwmNeg[FRONT_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis2Negative, joy), new JoystickInput(JoystickAxis.Axis4Negative, joy), new JoystickInput(JoystickAxis.Axis1Negative, joy));
                                buttons.pwmPos[BACK_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis2Negative, joy), new JoystickInput(JoystickAxis.Axis4Positive, joy), new JoystickInput(JoystickAxis.Axis1Negative, joy));
                                buttons.pwmNeg[BACK_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis2Positive, joy), new JoystickInput(JoystickAxis.Axis4Negative, joy), new JoystickInput(JoystickAxis.Axis1Positive, joy));
                            }

                            #endregion
                        }
                        break;
                    case Mode.Tank:
                        {
                            #region Tank Controls

                            // Player 1's tank controls are the same as the other players'
                            buttons.pwmPos[FRONT_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Negative, joy));
                            buttons.pwmNeg[FRONT_LEFT_PWM].set(new JoystickInput(JoystickAxis.Axis2Positive, joy));
                            buttons.pwmPos[FRONT_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis5Positive, joy));
                            buttons.pwmNeg[FRONT_RIGHT_PWM].set(new JoystickInput(JoystickAxis.Axis5Negative, joy));

                            #endregion
                        }
                        break;
                    default:
                        throw new UnhandledControlProfileException();
                }

                #endregion

                #region Other Controls

                if (player_i == 0) // Customized keyboard player 1 controls
                {
                    buttons.resetRobot = new KeyMapping("Reset Robot", KeyCode.R);
                    buttons.trajectory = new KeyMapping("Toggle Trajectory", KeyCode.T);
                    buttons.duplicateRobot = new KeyMapping("Duplicate Robot", KeyCode.U);
                    buttons.switchActiveRobot = new KeyMapping("Switch Active Robot", KeyCode.Y);
                }
                else
                {
                    buttons.resetRobot = new KeyMapping("Reset Robot", new JoystickInput(JoystickButton.Button8, joy));
                    buttons.trajectory = new KeyMapping("Toggle Trajectory", new JoystickInput(JoystickButton.Button11, joy));
                    buttons.duplicateRobot = new KeyMapping("Duplicate Robot", new JoystickInput(JoystickButton.Button1, joy));
                    buttons.switchActiveRobot = new KeyMapping("Switch Active Robot", new JoystickInput(JoystickButton.Button2, joy));
                }

                #endregion

                #endregion
            }
            else
            {
                throw new Exception("Failed to establish joystick index");
            }
            UpdateFieldControls(player_i);
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
                        buttons.spawnPieces.Add(new KeyMapping("1: Spawn " + FieldDataHandler.gamepieces[i].name, KeyCode.B));
                        buttons.pickup.Add(new KeyMapping("1: Pick Up " + FieldDataHandler.gamepieces[i].name, KeyCode.N));
                        buttons.release.Add(new KeyMapping("1: Release " + FieldDataHandler.gamepieces[i].name, KeyCode.M));

                    }
                    else
                    {
                        buttons.spawnPieces.Add(new KeyMapping("Spawn " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button5, joy)));
                        buttons.pickup.Add(new KeyMapping("Pick Up " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button3, joy)));
                        buttons.release.Add(new KeyMapping("Release " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button4, joy)));
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
