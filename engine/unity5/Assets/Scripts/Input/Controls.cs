using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Synthesis.Input.Inputs;
using Synthesis.Input.Enums;
using Synthesis.Field;

namespace Synthesis.Input
{
    public class Controls
    {
        public static Player[] Players;

        /// <summary>
        /// Initializes the <see cref="Controls"/> class.
        /// </summary>
        static Controls()
        {
            Players = new Player[Player.PLAYER_COUNT];
            for (int i = 0; i < Player.PLAYER_COUNT; i++)
            {
                Players[i] = new Player();
            }

            InitTankControls();
            InitArcadeControls();
        }

        private static void InitCommonControls(Player.ControlProfile controlProfile)
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                var key_i = player_i + 1;

                if (System.Enum.TryParse("Joystick" + key_i, false, out Joystick joy))
                {
                    #region All other player controls

                    //PWM controls
                    for (int pwm_i = 0; pwm_i < DriveJoints.PWM_COUNT - DriveJoints.PWM_OFFSET; pwm_i++)
                    {
                        var pwm_key_i = pwm_i + 1;
                        if (System.Enum.TryParse("Axis" + pwm_key_i + "Positive", false, out JoystickAxis axis_pos) &&
                            System.Enum.TryParse("Axis" + pwm_key_i + "Negative", false, out JoystickAxis axis_neg))
                        {
                            Players[player_i].buttons.pwmPos[pwm_i] = Players[player_i].SetKey(controlProfile, "PWM " + (pwm_i + DriveJoints.PWM_OFFSET) + " Positive", new JoystickInput(axis_pos, joy));
                            Players[player_i].buttons.pwmNeg[pwm_i] = Players[player_i].SetKey(controlProfile, "PWM " + (pwm_i + DriveJoints.PWM_OFFSET) + " Negative", new JoystickInput(axis_neg, joy));

                            Players[player_i].axes.pwmAxes[pwm_i] = Players[player_i].SetAxis(controlProfile, "PWM " + (pwm_i + DriveJoints.PWM_OFFSET) + " Axis " + (pwm_key_i + DriveJoints.PWM_OFFSET), Players[player_i].buttons.pwmNeg[pwm_i], Players[player_i].buttons.pwmPos[pwm_i]);
                        }
                        else
                        {
                            throw new System.Exception("Error configuring PWM buttons");
                        }
                    }

                    //Other Controls
                    Players[player_i].buttons.resetRobot = Players[player_i].SetKey(controlProfile, "Reset Robot", new JoystickInput(JoystickButton.Button8, joy));
                    Players[player_i].buttons.resetField = Players[player_i].SetKey(controlProfile, "Reset Field", new JoystickInput(JoystickButton.Button9, joy));
                    Players[player_i].buttons.replayMode = Players[player_i].SetKey(controlProfile, "Replay Mode", new JoystickInput(JoystickButton.Button12, joy));
                    Players[player_i].buttons.cameraToggle = Players[player_i].SetKey(controlProfile, "Camera Toggle", new JoystickInput(JoystickButton.Button7, joy));
                    Players[player_i].buttons.scoreboard = Players[player_i].SetKey(controlProfile, "Scoreboard", new JoystickInput(JoystickButton.Button10, joy));
                    Players[player_i].buttons.trajectory = Players[player_i].SetKey(controlProfile, "Toggle Trajectory", new JoystickInput(JoystickButton.Button11, joy));

                    #endregion
                }
                else
                {
                    throw new System.Exception("Failed to establish joystick index");
                }
            }
        }

        private static string MakePrefPrefix(int playerIndex, Player.ControlProfile controlProfile)
        {
            return "Controls.Player" + playerIndex + "." + controlProfile + ".";
        }

        /// <summary>
        /// Saves all primary and secondary controls.
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        public static void Save()
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                ReadOnlyCollection<KeyMapping> keys = Players[player_i].GetActiveList();
                var prefix = MakePrefPrefix(player_i, Players[player_i].GetActiveControlProfile());

                foreach (KeyMapping key in keys)
                {
                    PlayerPrefs.SetString(prefix + key.name + ".primary", key.primaryInput.ToString());
                    PlayerPrefs.SetString(prefix + key.name + ".secondary", key.secondaryInput.ToString());
                }
            }
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Checks if the user has saved their control settings by comparing strings.
        /// </summary>
        /// <returns>True: If user did save their controls
        ///          False: If the user hasn't saved their controls.
        /// </returns>
        public static bool CheckIfSaved()
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                ReadOnlyCollection<KeyMapping> currentKeys = Players[player_i].GetActiveList();
                var prefix = MakePrefPrefix(player_i, Players[player_i].GetActiveControlProfile());
                foreach (KeyMapping key in currentKeys)
                {
                    string lastString;
                    string inputString;

                    lastString = PlayerPrefs.GetString(prefix + key.name + ".primary");
                    inputString = key.primaryInput.ToString();

                    if (inputString != lastString)
                    {
                        return false;
                    }

                    lastString = PlayerPrefs.GetString(prefix + key.name + ".secondary");
                    inputString = key.secondaryInput.ToString();

                    if (inputString != lastString)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Loads all primary and secondary controls.
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        public static void Load()
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                ReadOnlyCollection<KeyMapping> keys = Players[player_i].GetActiveList();
                var prefix = MakePrefPrefix(player_i, Players[player_i].GetActiveControlProfile());
                foreach (KeyMapping key in keys)
                {
                    string inputStr;

                    inputStr = PlayerPrefs.GetString(prefix + key.name + ".primary");

                    if (inputStr != "")
                    {
                        key.primaryInput = CustomInputFromString(inputStr);
                    }

                    inputStr = PlayerPrefs.GetString(prefix + key.name + ".secondary");

                    if (inputStr != "")
                    {
                        key.secondaryInput = CustomInputFromString(inputStr);
                    }
                }
            }
        }


        /// <summary>
        /// Converts string representation of CustomInput to CustomInput.
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        /// <returns>CustomInput from string.</returns>
        /// <param name="value">String representation of CustomInput.</param>
        private static CustomInput CustomInputFromString(string value)
        {
            CustomInput res;

            res = JoystickInput.FromString(value);

            if (res != null)
            {
                return res;
            }

            res = MouseInput.FromString(value);

            if (res != null)
            {
                return res;
            }

            res = KeyboardInput.FromString(value);

            if (res != null)
            {
                return res;
            }

            return null;
        }

        public static void UpdateFieldControls(Player.ControlProfile controlProfile)
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                Players[player_i].buttons.pickup.Clear();
                Players[player_i].buttons.release.Clear();
                Players[player_i].buttons.spawnPieces.Clear();
            }
            for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
            {
                Players[0].buttons.pickup.Add(Players[0].SetKey(controlProfile, "Pick Up " + FieldDataHandler.gamepieces[i].name, KeyCode.LeftControl));
                Players[0].buttons.release.Add(Players[0].SetKey(controlProfile, "Release " + FieldDataHandler.gamepieces[i].name, KeyCode.LeftShift));
                Players[0].buttons.spawnPieces.Add(Players[0].SetKey(controlProfile, "Spawn " + FieldDataHandler.gamepieces[i].name, KeyCode.RightControl));

                for (int player_i = 1; player_i < Player.PLAYER_COUNT; player_i++)
                {
                    var key_i = player_i + 1;

                    if (System.Enum.TryParse("Joystick" + key_i, false, out Joystick joy))
                    {
                        Players[player_i].buttons.pickup.Add(Players[player_i].SetKey(controlProfile, "Pick Up " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button3, joy)));
                        Players[player_i].buttons.release.Add(Players[player_i].SetKey(controlProfile, "Release " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button4, joy)));
                        Players[player_i].buttons.spawnPieces.Add(Players[player_i].SetKey(controlProfile, "Spawn " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button5, joy)));
                    } else
                    {
                        throw new System.Exception("Failed to establish joystick index");
                    }
                }
            }
        }

        /// <summary>
        /// Default settings for ArcadeDrive 
        /// Adapted from: https://github.com/Gris87/InputControl
        /// </summary>
        private static void InitArcadeControls()
        {
            var controlProfile = Player.ControlProfile.ArcadeKeyboard;
            #region Player 1 Controls
            //Basic Controls
            Players[0].buttons.forward = Players[0].SetKey(controlProfile, "Forward", KeyCode.UpArrow);
            Players[0].buttons.backward = Players[0].SetKey(controlProfile, "Backward", KeyCode.DownArrow);
            Players[0].buttons.left = Players[0].SetKey(controlProfile, "Left", KeyCode.LeftArrow);
            Players[0].buttons.right = Players[0].SetKey(controlProfile, "Right", KeyCode.RightArrow);

            //Other Controls
            Players[0].buttons.resetRobot = Players[0].SetKey(controlProfile, "Reset Robot", KeyCode.R);
            Players[0].buttons.resetField = Players[0].SetKey(controlProfile, "Reset Field", KeyCode.F);
            Players[0].buttons.replayMode = Players[0].SetKey(controlProfile, "Replay Mode", KeyCode.Tab);
            Players[0].buttons.cameraToggle = Players[0].SetKey(controlProfile, "Camera Toggle", KeyCode.C);
            Players[0].buttons.scoreboard = Players[0].SetKey(controlProfile, "Scoreboard", KeyCode.Q);
            Players[0].buttons.trajectory = Players[0].SetKey(controlProfile, "Toggle Trajectory", KeyCode.T);
            Players[0].buttons.duplicateRobot = Players[0].SetKey(controlProfile, "Duplicate Robot", KeyCode.U);
            Players[0].buttons.switchActiveRobot = Players[0].SetKey(controlProfile, "Switch Active Robot", KeyCode.Y);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            Players[0].axes.horizontal =  Players[0].SetAxis(controlProfile, "Joystick 1 Axis 2", Players[0].buttons.left, Players[0].buttons.right);
            Players[0].axes.vertical =  Players[0].SetAxis(controlProfile, "Joystick 1 Axis 4", Players[0].buttons.backward, Players[0].buttons.forward);

            #endregion

            for (int player_i = 1; player_i < Player.PLAYER_COUNT; player_i++)
            {
                var key_i = player_i + 1;

                if (System.Enum.TryParse("Joystick" + key_i, false, out Joystick joy))
                {
                    #region All other player controls

                    //Basic robot controls
                    Players[player_i].buttons.forward = Players[player_i].SetKey(controlProfile, key_i + "Forward", new JoystickInput(JoystickAxis.Axis2Negative, joy));
                    Players[player_i].buttons.backward = Players[player_i].SetKey(controlProfile, key_i + "Backward", new JoystickInput(JoystickAxis.Axis2Positive, joy));
                    Players[player_i].buttons.left = Players[player_i].SetKey(controlProfile, key_i + "Left", new JoystickInput(JoystickAxis.Axis4Negative, joy));
                    Players[player_i].buttons.right = Players[player_i].SetKey(controlProfile, key_i + "Right", new JoystickInput(JoystickAxis.Axis4Positive, joy));

                    //Set Arcade Drive Axes (PWM [0] and PWM [1])
                    Players[player_i].axes.horizontal = Players[player_i].SetAxis(controlProfile, "Joystick " + key_i + " Axis 2", Players[player_i].buttons.left, Players[player_i].buttons.right);
                    Players[player_i].axes.vertical = Players[player_i].SetAxis(controlProfile, "Joystick " + key_i + " Axis 4", Players[player_i].buttons.backward, Players[player_i].buttons.forward);
                    #endregion
                }
                else
                {
                    throw new System.Exception("Failed to establish joystick index");
                }
            }
            InitCommonControls(controlProfile);
            UpdateFieldControls(controlProfile);
        }

        /// <summary>
        /// Default settings for TankDrive controls.
        /// Adapted from: https://github.com/Gris87/InputControl
        /// </summary>
        private static void InitTankControls()
        {
            var controlProfile = Player.ControlProfile.TankJoystick;
            #region Player 1 Controls
            //Tank controls
            Players[0].buttons.tankFrontLeft = Players[0].SetKey(controlProfile, "Tank Front Left", new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick1));
            Players[0].buttons.tankBackLeft = Players[0].SetKey(controlProfile, "Tank Back Left", new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick1));
            Players[0].buttons.tankFrontRight = Players[0].SetKey(controlProfile, "Tank Front Right", new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick1));
            Players[0].buttons.tankBackRight = Players[0].SetKey(controlProfile, "Tank Back Right", new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick1));

            //Other Controls
            Players[0].buttons.resetRobot = Players[0].SetKey(controlProfile, "Reset Robot", KeyCode.R);
            Players[0].buttons.resetField = Players[0].SetKey(controlProfile, "Reset Field", KeyCode.F);
            Players[0].buttons.replayMode = Players[0].SetKey(controlProfile, "Replay Mode", KeyCode.Tab);
            Players[0].buttons.cameraToggle = Players[0].SetKey(controlProfile, "Camera Toggle", KeyCode.C);
            Players[0].buttons.scoreboard = Players[0].SetKey(controlProfile, "Scoreboard", KeyCode.Q);
            Players[0].buttons.trajectory = Players[0].SetKey(controlProfile, "Toggle Trajectory", KeyCode.T);
            Players[0].buttons.duplicateRobot = Players[0].SetKey(controlProfile, "Duplicate Robot", KeyCode.U);
            Players[0].buttons.switchActiveRobot = Players[0].SetKey(controlProfile, "Switch Active Robot", KeyCode.Y);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            Players[0].axes.tankLeftAxes =  Players[0].SetAxis(controlProfile, "Joystick 1 Axis 9", Players[0].buttons.tankBackLeft, Players[0].buttons.tankFrontLeft);
            Players[0].axes.tankRightAxes =  Players[0].SetAxis(controlProfile, "Joystick 1 Axis 10", Players[0].buttons.tankFrontRight, Players[0].buttons.tankBackRight);

            #endregion

            for (int player_i = 1; player_i < Player.PLAYER_COUNT; player_i++)
            {
                var key_i = player_i + 1;

                if (System.Enum.TryParse("Joystick" + key_i, false, out Joystick joy))
                {
                    #region All Other Players' Controls
                    //Tank Controls
                    Players[player_i].buttons.tankFrontLeft = Players[player_i].SetKey(controlProfile, "Tank Front Left", new JoystickInput(JoystickAxis.Axis2Negative, joy));
                    Players[player_i].buttons.tankBackLeft = Players[player_i].SetKey(controlProfile, "Tank Back Left", new JoystickInput(JoystickAxis.Axis2Positive, joy));
                    Players[player_i].buttons.tankFrontRight = Players[player_i].SetKey(controlProfile, "Tank Front Right", new JoystickInput(JoystickAxis.Axis5Negative, joy));
                    Players[player_i].buttons.tankBackRight = Players[player_i].SetKey(controlProfile, "Tank Back Right", new JoystickInput(JoystickAxis.Axis5Positive, joy));

                    //Set Tank Drive Axes (PWM [0] and PWM [1])
                    Players[player_i].axes.tankLeftAxes = Players[player_i].SetAxis(controlProfile, "Joystick " + key_i + " Axis 9", Players[player_i].buttons.tankBackLeft, Players[player_i].buttons.tankFrontLeft);
                    Players[player_i].axes.tankRightAxes = Players[player_i].SetAxis(controlProfile, "Joystick " + key_i + " Axis 10", Players[player_i].buttons.tankFrontRight, Players[player_i].buttons.tankBackRight);
                    #endregion
                }
                else
                {
                    throw new System.Exception("Failed to establish joystick index");
                }
            }
            InitCommonControls(controlProfile);
            UpdateFieldControls(controlProfile);
        }
    }
}