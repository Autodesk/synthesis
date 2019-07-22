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
        public static int PLAYER_COUNT = 6;
        public static Player[] Players;


        public static bool TankDriveEnabled;    //Checks if TankDrive is enabled

        /// <summary>
        /// <see cref="Buttons"/> is a set of user defined buttons.
        /// </summary>
        public struct Buttons
        {
            //Basic robot controls
            public KeyMapping forward;
            public KeyMapping backward;
            public KeyMapping left;
            public KeyMapping right;

            //Tank drive controls
            public KeyMapping tankFrontLeft;
            public KeyMapping tankBackLeft;
            public KeyMapping tankFrontRight;
            public KeyMapping tankBackRight;

            //Remaining PWM Controls
            public KeyMapping pwm2Plus;
            public KeyMapping pwm2Neg;
            public KeyMapping pwm3Plus;
            public KeyMapping pwm3Neg;
            public KeyMapping pwm4Plus;
            public KeyMapping pwm4Neg;
            public KeyMapping pwm5Plus;
            public KeyMapping pwm5Neg;
            public KeyMapping pwm6Plus;
            public KeyMapping pwm6Neg;
            public KeyMapping pwm7Plus;
            public KeyMapping pwm7Neg;
            public KeyMapping pwm8Plus;
            public KeyMapping pwm8Neg;
            public KeyMapping pwm9Plus;
            public KeyMapping pwm9Neg;

            //Other controls
            public KeyMapping resetRobot;
            public KeyMapping resetField;
            public KeyMapping cameraToggle;
            public KeyMapping scoreboard;
            public KeyMapping trajectory;
            public KeyMapping replayMode;
            public KeyMapping duplicateRobot;
            public KeyMapping switchActiveRobot;

            //driver practice controls
            public List<KeyMapping> pickup;
            public List<KeyMapping> release;
            public List<KeyMapping> spawnPieces;


        }

        /// <summary>
        /// <see cref="Axes"/> is a set of user defined axes.
        /// </summary>
        public struct Axes
        {
            //Arcade Axes
            public Axis vertical;
            public Axis horizontal;

            //Tank Axes
            public Axis tankLeftAxes;
            public Axis tankRightAxes;

            //PWM Axes
            public Axis pwm2Axes;
            public Axis pwm3Axes;
            public Axis pwm4Axes;
            public Axis pwm5Axes;
            public Axis pwm6Axes;
            public Axis pwm7Axes;
            public Axis pwm8Axes;
            public Axis pwm9Axes;
        }

        /// <summary>
        /// Set of buttons.
        /// </summary>
        public static Buttons[] buttons = new Buttons[6];

        /// <summary>
        /// Set of axes.
        /// </summary>
        public static Axes[] axes = new Axes[6];

        /// <summary>
        /// Initializes the <see cref="Controls"/> class.
        /// </summary>
        static Controls()
        {
            Players = new Player[PLAYER_COUNT];
            for (int i = 0; i < PLAYER_COUNT; i++)
            {
                Players[i] = new Player();
            }

            TankControls();
            ArcadeControls();
        }

        private static void CommonControls(bool isTank)
        {
            for (int player_i = 1; player_i < PLAYER_COUNT; player_i++)
            {
                var key_i = player_i + 1;

                if (System.Enum.TryParse("Joystick" + key_i, false, out Joystick joy))
                {
                    #region All other player controls

                    //PWM controls
                    buttons[player_i].pwm2Plus = Controls.Players[player_i].SetKey(key_i + ": PWM 2 Positive", new JoystickInput(JoystickAxis.Axis3Positive, joy), isTank);
                    buttons[player_i].pwm2Neg = Controls.Players[player_i].SetKey(key_i + ": PWM 2 Negative", new JoystickInput(JoystickAxis.Axis3Negative, joy), isTank);
                    buttons[player_i].pwm3Plus = Controls.Players[player_i].SetKey(key_i + ": PWM 3 Positive", new JoystickInput(JoystickAxis.Axis5Positive, joy), isTank);
                    buttons[player_i].pwm3Neg = Controls.Players[player_i].SetKey(key_i + ": PWM 3 Negative", new JoystickInput(JoystickAxis.Axis5Negative, joy), isTank);
                    buttons[player_i].pwm4Plus = Controls.Players[player_i].SetKey(key_i + ": PWM 4 Positive", new JoystickInput(JoystickAxis.Axis6Positive, joy), isTank);
                    buttons[player_i].pwm4Neg = Controls.Players[player_i].SetKey(key_i + ": PWM 4 Negative", new JoystickInput(JoystickAxis.Axis6Negative, joy), isTank);
                    buttons[player_i].pwm5Plus = Controls.Players[player_i].SetKey(key_i + ": PWM 5 Positive", new JoystickInput(JoystickAxis.Axis7Positive, joy), isTank);
                    buttons[player_i].pwm5Neg = Controls.Players[player_i].SetKey(key_i + ": PWM 5 Negative", new JoystickInput(JoystickAxis.Axis7Negative, joy), isTank);
                    buttons[player_i].pwm6Plus = Controls.Players[player_i].SetKey(key_i + ": PWM 6 Positive", new JoystickInput(JoystickAxis.Axis8Positive, joy), isTank);
                    buttons[player_i].pwm6Neg = Controls.Players[player_i].SetKey(key_i + ": PWM 6 Negative", new JoystickInput(JoystickAxis.Axis8Negative, joy), isTank);
                    buttons[player_i].pwm7Plus = Controls.Players[player_i].SetKey(key_i + ": PWM 7 Positive", new JoystickInput(JoystickAxis.Axis9Positive, joy), isTank);
                    buttons[player_i].pwm7Neg = Controls.Players[player_i].SetKey(key_i + ": PWM 7 Negative", new JoystickInput(JoystickAxis.Axis9Negative, joy), isTank);
                    buttons[player_i].pwm8Plus = Controls.Players[player_i].SetKey(key_i + ": PWM 8 Positive", new JoystickInput(JoystickAxis.Axis10Positive, joy), isTank);
                    buttons[player_i].pwm8Neg = Controls.Players[player_i].SetKey(key_i + ": PWM 8 Negative", new JoystickInput(JoystickAxis.Axis10Negative, joy), isTank);
                    buttons[player_i].pwm9Plus = Controls.Players[player_i].SetKey(key_i + ": PWM 9 Positive", new JoystickInput(JoystickAxis.Axis11Positive, joy), isTank);
                    buttons[player_i].pwm9Neg = Controls.Players[player_i].SetKey(key_i + ": PWM 9 Negative", new JoystickInput(JoystickAxis.Axis11Negative, joy), isTank);

                    //Other Controls
                    buttons[player_i].resetRobot = Controls.Players[player_i].SetKey(key_i + ": Reset Robot", new JoystickInput(JoystickButton.Button8, joy), isTank);
                    buttons[player_i].resetField = Controls.Players[player_i].SetKey(key_i + ": Reset Field", new JoystickInput(JoystickButton.Button9, joy), isTank);
                    buttons[player_i].replayMode = Controls.Players[player_i].SetKey(key_i + ": Replay Mode", new JoystickInput(JoystickButton.Button12, joy), isTank);
                    buttons[player_i].cameraToggle = Controls.Players[player_i].SetKey(key_i + ": Camera Toggle", new JoystickInput(JoystickButton.Button7, joy), isTank);
                    buttons[player_i].scoreboard = Controls.Players[player_i].SetKey(key_i + ": Scoreboard", new JoystickInput(JoystickButton.Button10, joy), isTank);
                    buttons[player_i].trajectory = Controls.Players[player_i].SetKey(key_i + ": Toggle Trajectory", new JoystickInput(JoystickButton.Button11, joy), isTank);
                    buttons[player_i].pickup = new List<KeyMapping>();
                    buttons[player_i].release = new List<KeyMapping>();
                    buttons[player_i].spawnPieces = new List<KeyMapping>();
                    for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
                    {
                        buttons[player_i].pickup.Add(Controls.Players[player_i].SetKey(key_i + ": Pick Up " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button3, joy), isTank));
                        buttons[player_i].release.Add(Controls.Players[player_i].SetKey(key_i + ": Release " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button4, joy), isTank));
                        buttons[player_i].spawnPieces.Add(Controls.Players[player_i].SetKey(key_i + ": Spawn " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button5, joy), isTank));
                    }

                    //Set PWM Axes
                    axes[player_i].pwm2Axes = Controls.Players[player_i].SetAxis(key_i + ": PWM 2 Axis 3", buttons[player_i].pwm2Neg, buttons[player_i].pwm2Plus, isTank);
                    axes[player_i].pwm3Axes = Controls.Players[player_i].SetAxis(key_i + ": PWM 3 Axis 5", buttons[player_i].pwm3Neg, buttons[player_i].pwm3Plus, isTank);
                    axes[player_i].pwm4Axes = Controls.Players[player_i].SetAxis(key_i + ": PWM 4 Axis 6", buttons[player_i].pwm4Neg, buttons[player_i].pwm4Plus, isTank);
                    axes[player_i].pwm5Axes = Controls.Players[player_i].SetAxis(key_i + ": PWM 5 Axis 7", buttons[player_i].pwm5Neg, buttons[player_i].pwm5Plus, isTank);
                    axes[player_i].pwm6Axes = Controls.Players[player_i].SetAxis(key_i + ": PWM 6 Axis 8", buttons[player_i].pwm6Neg, buttons[player_i].pwm6Plus, isTank);
                    axes[player_i].pwm7Axes = Controls.Players[player_i].SetAxis(key_i + ": PWM 7 Axis 9", buttons[player_i].pwm7Neg, buttons[player_i].pwm7Plus, isTank);
                    axes[player_i].pwm8Axes = Controls.Players[player_i].SetAxis(key_i + ": PWM 8 Axis 10", buttons[player_i].pwm8Neg, buttons[player_i].pwm8Plus, isTank);
                    axes[player_i].pwm9Axes = Controls.Players[player_i].SetAxis(key_i + ": PWM 9 Axis 11", buttons[player_i].pwm9Neg, buttons[player_i].pwm9Plus, isTank);
                    #endregion
                }
                else
                {
                    throw new System.Exception("Failed to establish joystick index");
                }
            }
        }

        /// <summary>
        /// Saves all primary and secondary controls.
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        public static void Save()
        {
            ReadOnlyCollection<KeyMapping> keys = InputControl.GetKeysList();

            foreach (KeyMapping key in keys)
            {
                PlayerPrefs.SetString("Controls." + key.name + ".primary", key.primaryInput.ToString());
                PlayerPrefs.SetString("Controls." + key.name + ".secondary", key.secondaryInput.ToString());
            }
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Checks if the user has saved their control settings by comparing strings.
        /// </summary>
        /// <returns>True: If user did not save their controls
        ///          False: If the user saved their controls.
        /// </returns>
        public static bool CheckIfSaved()
        {
            ReadOnlyCollection<KeyMapping> currentKeys = InputControl.GetKeysList();

            foreach (KeyMapping key in currentKeys)
            {
                string lastString;
                string inputString;

                lastString = PlayerPrefs.GetString("Controls." + key.name + ".primary");
                inputString = key.primaryInput.ToString();

                if (inputString != lastString)
                {
                    return true;
                }

                lastString = PlayerPrefs.GetString("Controls." + key.name + ".secondary");
                inputString = key.secondaryInput.ToString();

                if (inputString != lastString)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Loads all primary and secondary controls.
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        public static void Load()
        {
            ReadOnlyCollection<KeyMapping> keys = InputControl.GetKeysList();

            foreach (KeyMapping key in keys)
            {
                string inputStr;

                inputStr = PlayerPrefs.GetString("Controls." + key.name + ".primary");

                if (inputStr != "")
                {
                    key.primaryInput = customInputFromString(inputStr);
                }

                inputStr = PlayerPrefs.GetString("Controls." + key.name + ".secondary");

                if (inputStr != "")
                {
                    key.secondaryInput = customInputFromString(inputStr);
                }
            }
        }


        /// <summary>
        /// Converts string representation of CustomInput to CustomInput.
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        /// <returns>CustomInput from string.</returns>
        /// <param name="value">String representation of CustomInput.</param>
        public static CustomInput customInputFromString(string value)
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

        public static void UpdateFieldControls(bool isTank)
        {
            for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
            {
                for (int player_i = 0; player_i < PLAYER_COUNT; player_i++)
                {
                    buttons[player_i].pickup = new List<KeyMapping>();
                    buttons[player_i].release = new List<KeyMapping>();
                    buttons[player_i].spawnPieces = new List<KeyMapping>();
                }

                buttons[0].pickup.Add(Controls.Players[0].SetKey("1: Pick Up " + FieldDataHandler.gamepieces[i].name, KeyCode.LeftControl, isTank));
                buttons[0].release.Add(Controls.Players[0].SetKey("1: Release " + FieldDataHandler.gamepieces[i].name, KeyCode.LeftShift, isTank));
                buttons[0].spawnPieces.Add(Controls.Players[0].SetKey("1: Spawn " + FieldDataHandler.gamepieces[i].name, KeyCode.RightControl, isTank));

                for (int player_i = 1; player_i < PLAYER_COUNT; player_i++)
                {
                    buttons[player_i].pickup = new List<KeyMapping>();
                    buttons[player_i].release = new List<KeyMapping>();
                    buttons[player_i].spawnPieces = new List<KeyMapping>();

                    var key_i = player_i + 1;

                    if (System.Enum.TryParse("Joystick" + key_i, false, out Joystick joy))
                    {
                        buttons[player_i].pickup.Add(Controls.Players[player_i].SetKey(key_i + ": Pick Up " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button3, joy), isTank));
                        buttons[player_i].release.Add(Controls.Players[player_i].SetKey(key_i + ": Release " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button4, joy), isTank));
                        buttons[player_i].spawnPieces.Add(Controls.Players[player_i].SetKey(key_i + ": Spawn " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button5, joy), isTank));
                    } else
                    {
                        throw new System.Exception("Failed to establish joystick index");
                    }
                }
            }
        }

        /// <summary>
        /// Default settings for ArcadeDrive controls.
        /// Adapted from: https://github.com/Gris87/InputControl
        /// </summary>
        private static void ArcadeControls()
        {
            CommonControls(false);

            #region Player 1 Controls
            //Basic Controls
            buttons[0].forward = Controls.Players[0].SetKey("1: Forward", KeyCode.UpArrow, false);
            buttons[0].backward = Controls.Players[0].SetKey("1: Backward", KeyCode.DownArrow, false);
            buttons[0].left = Controls.Players[0].SetKey("1: Left", KeyCode.LeftArrow, false);
            buttons[0].right = Controls.Players[0].SetKey("1: Right", KeyCode.RightArrow, false);

            //Remaining PWM controls
            buttons[0].pwm2Plus = Controls.Players[0].SetKey("1: PWM 2 Positive", KeyCode.Alpha1, false);
            buttons[0].pwm2Neg = Controls.Players[0].SetKey("1: PWM 2 Negative", KeyCode.Alpha2, false);
            buttons[0].pwm3Plus = Controls.Players[0].SetKey("1: PWM 3 Positive", KeyCode.Alpha3, false);
            buttons[0].pwm3Neg = Controls.Players[0].SetKey("1: PWM 3 Negative", KeyCode.Alpha4, false);
            buttons[0].pwm4Plus = Controls.Players[0].SetKey("1: PWM 4 Positive", KeyCode.Alpha5, false);
            buttons[0].pwm4Neg = Controls.Players[0].SetKey("1: PWM 4 Negative", KeyCode.Alpha6, false);
            buttons[0].pwm5Plus = Controls.Players[0].SetKey("1: PWM 5 Positive", KeyCode.Alpha7, false);
            buttons[0].pwm5Neg = Controls.Players[0].SetKey("1: PWM 5 Negative", KeyCode.Alpha8, false);
            buttons[0].pwm6Plus = Controls.Players[0].SetKey("1: PWM 6 Positive", KeyCode.Alpha9, false);
            buttons[0].pwm6Neg = Controls.Players[0].SetKey("1: PWM 6 Negative", KeyCode.Alpha0, false);
            buttons[0].pwm7Plus = Controls.Players[0].SetKey("1: PWM 7 Positive", KeyCode.Slash, false);
            buttons[0].pwm7Neg = Controls.Players[0].SetKey("1: PWM 7 Negative", KeyCode.Period, false);
            buttons[0].pwm8Plus = Controls.Players[0].SetKey("1: PWM 8 Positive", KeyCode.Comma, false);
            buttons[0].pwm8Neg = Controls.Players[0].SetKey("1: PWM 8 Negative", KeyCode.M, false);
            buttons[0].pwm9Plus = Controls.Players[0].SetKey("1: PWM 9 Positive", KeyCode.N, false);
            buttons[0].pwm9Neg = Controls.Players[0].SetKey("1: PWM 9 Negative", KeyCode.B, false);

            //Other Controls
            buttons[0].resetRobot = Controls.Players[0].SetKey("1: Reset Robot", KeyCode.R, false);
            buttons[0].resetField = Controls.Players[0].SetKey("1: Reset Field", KeyCode.F, false);
            buttons[0].replayMode = Controls.Players[0].SetKey("1: Replay Mode", KeyCode.Tab, false);
            buttons[0].cameraToggle = Controls.Players[0].SetKey("1: Camera Toggle", KeyCode.C, false);
            buttons[0].scoreboard = Controls.Players[0].SetKey("1: Scoreboard", KeyCode.Q, false);
            buttons[0].trajectory = Controls.Players[0].SetKey("1: Toggle Trajectory", KeyCode.T, false);
            buttons[0].duplicateRobot = Controls.Players[0].SetKey("1: Duplicate Robot", KeyCode.U, false);
            buttons[0].switchActiveRobot = Controls.Players[0].SetKey("1: Switch Active Robot", KeyCode.Y, false);

            //driver practice controls - dependent on number of gamepieces
            buttons[0].pickup = new List<KeyMapping>();
            buttons[0].release = new List<KeyMapping>();
            buttons[0].spawnPieces = new List<KeyMapping>();
            for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
            {
                buttons[0].pickup.Add(Controls.Players[0].SetKey("1: Pick Up " + FieldDataHandler.gamepieces[i].name, KeyCode.LeftControl, false));
                buttons[0].release.Add(Controls.Players[0].SetKey("1: Release " + FieldDataHandler.gamepieces[i].name, KeyCode.LeftShift, false));
                buttons[0].spawnPieces.Add(Controls.Players[0].SetKey("1: Spawn " + FieldDataHandler.gamepieces[i].name, KeyCode.RightControl, false));
            }

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[0].horizontal =  Controls.Players[0].SetAxis("1: Joystick 1 Axis 2", buttons[0].left, buttons[0].right, false);
            axes[0].vertical =  Controls.Players[0].SetAxis("1: Joystick 1 Axis 4", buttons[0].backward, buttons[0].forward, false);

            //Set PWM Axes
            axes[0].pwm2Axes =  Controls.Players[0].SetAxis("1: PWM 2 Axis 3", buttons[0].pwm2Neg, buttons[0].pwm2Plus, false);
            axes[0].pwm3Axes =  Controls.Players[0].SetAxis("1: PWM 3 Axis 5", buttons[0].pwm3Neg, buttons[0].pwm3Plus, false);
            axes[0].pwm4Axes =  Controls.Players[0].SetAxis("1: PWM 4 Axis 6", buttons[0].pwm4Neg, buttons[0].pwm4Plus, false);
            axes[0].pwm5Axes =  Controls.Players[0].SetAxis("1: PWM 5 Axis 7", buttons[0].pwm5Neg, buttons[0].pwm5Plus, false);
            axes[0].pwm6Axes =  Controls.Players[0].SetAxis("1: PWM 6 Axis 8", buttons[0].pwm6Neg, buttons[0].pwm6Plus, false);
            axes[0].pwm7Axes =  Controls.Players[0].SetAxis("1: PWM 7 Axis 9", buttons[0].pwm7Neg, buttons[0].pwm7Plus, false);
            axes[0].pwm8Axes =  Controls.Players[0].SetAxis("1: PWM 8 Axis 10", buttons[0].pwm8Neg, buttons[0].pwm8Plus, false);
            axes[0].pwm9Axes =  Controls.Players[0].SetAxis("1: PWM 9 Axis 11", buttons[0].pwm9Neg, buttons[0].pwm9Plus, false);

            #endregion

            for (int player_i = 1; player_i < PLAYER_COUNT; player_i++)
            {
                var key_i = player_i + 1;

                if (System.Enum.TryParse("Joystick" + key_i, false, out Joystick joy))
                {
                    #region All other player controls

                    //Basic robot controls
                    buttons[player_i].forward = Controls.Players[player_i].SetKey(key_i + ": Forward", new JoystickInput(JoystickAxis.Axis2Negative, joy), false);
                    buttons[player_i].backward = Controls.Players[player_i].SetKey(key_i + ": Backward", new JoystickInput(JoystickAxis.Axis2Positive, joy), false);
                    buttons[player_i].left = Controls.Players[player_i].SetKey(key_i + ": Left", new JoystickInput(JoystickAxis.Axis4Negative, joy), false);
                    buttons[player_i].right = Controls.Players[player_i].SetKey(key_i + ": Right", new JoystickInput(JoystickAxis.Axis4Positive, joy), false);

                    //Set Arcade Drive Axes (PWM [0] and PWM [1])
                    axes[player_i].horizontal = Controls.Players[player_i].SetAxis("Joystick " + key_i + " Axis 2", buttons[player_i].left, buttons[player_i].right, false);
                    axes[player_i].vertical = Controls.Players[player_i].SetAxis("Joystick " + key_i + " Axis 4", buttons[player_i].backward, buttons[player_i].forward, false);
                    #endregion
                }
                else
                {
                    throw new System.Exception("Failed to establish joystick index");
                }
            }
        }

        /// <summary>
        /// Default settings for TankDrive controls.
        /// Adapted from: https://github.com/Gris87/InputControl
        /// </summary>
        private static void TankControls()
        {
            CommonControls(true);
            #region Player 1 Controls
            //Tank controls
            buttons[0].tankFrontLeft = Controls.Players[0].SetKey("1: Tank Front Left", new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick1), true);
            buttons[0].tankBackLeft = Controls.Players[0].SetKey("1: Tank Back Left", new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick1), true);
            buttons[0].tankFrontRight = Controls.Players[0].SetKey("1: Tank Front Right", new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick1), true);
            buttons[0].tankBackRight = Controls.Players[0].SetKey("1: Tank Back Right", new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick1), true);

            //Remaining PWM controls
            buttons[0].pwm2Plus = Controls.Players[0].SetKey("1: PWM 2 Positive", KeyCode.Alpha1, true);
            buttons[0].pwm2Neg = Controls.Players[0].SetKey("1: PWM 2 Negative", KeyCode.Alpha2, true);
            buttons[0].pwm3Plus = Controls.Players[0].SetKey("1: PWM 3 Positive", KeyCode.Alpha3, true);
            buttons[0].pwm3Neg = Controls.Players[0].SetKey("1: PWM 3 Negative", KeyCode.Alpha4, true);
            buttons[0].pwm4Plus = Controls.Players[0].SetKey("1: PWM 4 Positive", KeyCode.Alpha5, true);
            buttons[0].pwm4Neg = Controls.Players[0].SetKey("1: PWM 4 Negative", KeyCode.Alpha6, true);
            buttons[0].pwm5Plus = Controls.Players[0].SetKey("1: PWM 5 Positive", KeyCode.Alpha7, true);
            buttons[0].pwm5Neg = Controls.Players[0].SetKey("1: PWM 5 Negative", KeyCode.Alpha8, true);
            buttons[0].pwm6Plus = Controls.Players[0].SetKey("1: PWM 6 Positive", KeyCode.Alpha9, true);
            buttons[0].pwm6Neg = Controls.Players[0].SetKey("1: PWM 6 Negative", KeyCode.Alpha0, true);
            buttons[0].pwm7Plus = Controls.Players[0].SetKey("1: PWM 7 Positive", KeyCode.Slash, true);
            buttons[0].pwm7Neg = Controls.Players[0].SetKey("1: PWM 7 Negative", KeyCode.Period, true);
            buttons[0].pwm8Plus = Controls.Players[0].SetKey("1: PWM 8 Positive", KeyCode.Comma, true);
            buttons[0].pwm8Neg = Controls.Players[0].SetKey("1: PWM 8 Negative", KeyCode.M, true);
            buttons[0].pwm9Plus = Controls.Players[0].SetKey("1: PWM 9 Positive", KeyCode.N, true);
            buttons[0].pwm9Neg = Controls.Players[0].SetKey("1: PWM 9 Negative", KeyCode.B, true);

            //Other Controls
            buttons[0].resetRobot = Controls.Players[0].SetKey("1: Reset Robot", KeyCode.R, true);
            buttons[0].resetField = Controls.Players[0].SetKey("1: Reset Field", KeyCode.F, true);
            buttons[0].replayMode = Controls.Players[0].SetKey("1: Replay Mode", KeyCode.Tab, true);
            buttons[0].cameraToggle = Controls.Players[0].SetKey("1: Camera Toggle", KeyCode.C, true);
            buttons[0].scoreboard = Controls.Players[0].SetKey("1: Scoreboard", KeyCode.Q, true);
            buttons[0].trajectory = Controls.Players[0].SetKey("1: Toggle Trajectory", KeyCode.T, true);
            buttons[0].duplicateRobot = Controls.Players[0].SetKey("1: Duplicate Robot", KeyCode.U, true);
            buttons[0].switchActiveRobot = Controls.Players[0].SetKey("1: Switch Active Robot", KeyCode.Y, true);

            //driver practice controls - dependent on number of gamepieces
            buttons[0].pickup = new List<KeyMapping>();
            buttons[0].release = new List<KeyMapping>();
            buttons[0].spawnPieces = new List<KeyMapping>();
            for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
            {
                buttons[0].pickup.Add(Controls.Players[0].SetKey("1: Pick Up " + FieldDataHandler.gamepieces[i].name, KeyCode.LeftControl, true));
                buttons[0].release.Add(Controls.Players[0].SetKey("1: Release " + FieldDataHandler.gamepieces[i].name, KeyCode.LeftShift, true));
                buttons[0].spawnPieces.Add(Controls.Players[0].SetKey("1: Spawn " + FieldDataHandler.gamepieces[i].name, KeyCode.RightControl, true));
            }

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[0].tankLeftAxes =  Controls.Players[0].SetAxis("1: Joystick 1 Axis 9", buttons[0].tankBackLeft, buttons[0].tankFrontLeft, true);
            axes[0].tankRightAxes =  Controls.Players[0].SetAxis("1: Joystick 1 Axis 10", buttons[0].tankFrontRight, buttons[0].tankBackRight, true);

            //Set PWM Axes
            axes[0].pwm2Axes =  Controls.Players[0].SetAxis("1: PWM 2 Axis 3", buttons[0].pwm2Neg, buttons[0].pwm2Plus, true);
            axes[0].pwm3Axes =  Controls.Players[0].SetAxis("1: PWM 3 Axis 5", buttons[0].pwm3Neg, buttons[0].pwm3Plus, true);
            axes[0].pwm4Axes =  Controls.Players[0].SetAxis("1: PWM 4 Axis 6", buttons[0].pwm4Neg, buttons[0].pwm4Plus, true);
            axes[0].pwm5Axes =  Controls.Players[0].SetAxis("1: PWM 5 Axis 7", buttons[0].pwm5Neg, buttons[0].pwm5Plus, true);
            axes[0].pwm6Axes =  Controls.Players[0].SetAxis("1: PWM 6 Axis 8", buttons[0].pwm6Neg, buttons[0].pwm6Plus, true);
            axes[0].pwm7Axes =  Controls.Players[0].SetAxis("1: PWM 7 Axis 9", buttons[0].pwm7Neg, buttons[0].pwm7Plus, true);
            axes[0].pwm8Axes =  Controls.Players[0].SetAxis("1: PWM 8 Axis 10", buttons[0].pwm8Neg, buttons[0].pwm8Plus, true);
            axes[0].pwm9Axes =  Controls.Players[0].SetAxis("1: PWM 9 Axis 11", buttons[0].pwm9Neg, buttons[0].pwm9Plus, true);
            #endregion

            for (int player_i = 1; player_i < PLAYER_COUNT; player_i++)
            {
                var key_i = player_i + 1;

                if (System.Enum.TryParse("Joystick" + key_i, false, out Joystick joy))
                {
                    #region All Other Players' Controls
                    //Tank Controls
                    buttons[player_i].tankFrontLeft = Controls.Players[player_i].SetKey(key_i + ": Tank Front Left", new JoystickInput(JoystickAxis.Axis2Negative, joy), true);
                    buttons[player_i].tankBackLeft = Controls.Players[player_i].SetKey(key_i + ": Tank Back Left", new JoystickInput(JoystickAxis.Axis2Positive, joy), true);
                    buttons[player_i].tankFrontRight = Controls.Players[player_i].SetKey(key_i + ": Tank Front Right", new JoystickInput(JoystickAxis.Axis5Negative, joy), true);
                    buttons[player_i].tankBackRight = Controls.Players[player_i].SetKey(key_i + ": Tank Back Right", new JoystickInput(JoystickAxis.Axis5Positive, joy), true);

                    //Set Tank Drive Axes (PWM [0] and PWM [1])
                    axes[player_i].tankLeftAxes = Controls.Players[player_i].SetAxis("Joystick " + key_i + " Axis 9", buttons[player_i].tankBackLeft, buttons[player_i].tankFrontLeft, true);
                    axes[player_i].tankRightAxes = Controls.Players[player_i].SetAxis("Joystick " + key_i + " Axis 10", buttons[player_i].tankFrontRight, buttons[player_i].tankBackRight, true);
                    #endregion
                }
                else
                {
                    throw new System.Exception("Failed to establish joystick index");
                }
            }
        }
    }
}