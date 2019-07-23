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
        public static int PWM_COUNT = 10;
        public static int PWM_OFFSET = 2;

        public static int PLAYER_COUNT = 6;
        public static Player[] Players;

        /// <summary>
        /// <see cref="Buttons"/> is a set of user defined buttons.
        /// </summary>
        public class Buttons
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
            public KeyMapping[] pwmPos;
            public KeyMapping[] pwmNeg;

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

            public Buttons()
            {
                forward = new KeyMapping();
                backward = new KeyMapping();
                left = new KeyMapping();
                right = new KeyMapping();

                tankFrontLeft = new KeyMapping();
                tankBackLeft = new KeyMapping();
                tankFrontRight = new KeyMapping();
                tankBackRight = new KeyMapping();

                pwmPos = new KeyMapping[PWM_COUNT - PWM_OFFSET];
                for (var i = 0; i < pwmPos.Length; i++)
                    pwmPos[i] = new KeyMapping();

                pwmNeg = new KeyMapping[PWM_COUNT - PWM_OFFSET];
                for (var i = 0; i < pwmNeg.Length; i++)
                    pwmNeg[i] = new KeyMapping();

                resetRobot = new KeyMapping();
                resetField = new KeyMapping();
                cameraToggle = new KeyMapping();
                scoreboard = new KeyMapping();
                trajectory = new KeyMapping();
                replayMode = new KeyMapping();
                duplicateRobot = new KeyMapping();
                switchActiveRobot = new KeyMapping();

                pickup = new List<KeyMapping>();
                release = new List<KeyMapping>();
                spawnPieces = new List<KeyMapping>();
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
                vertical = null;
                horizontal = null;
                tankLeftAxes = null;
                tankRightAxes = null;

                pwmAxes = new Axis[PWM_COUNT - PWM_OFFSET];
            }
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
                buttons[i] = new Buttons();
                axes[i] = new Axes();
            }

            TankControls();
            ArcadeControls();
        }

        private static void CommonControls(Player.ControlProfile controlProfile)
        {
            for (int player_i = 0; player_i < PLAYER_COUNT; player_i++)
            {
                var key_i = player_i + 1;

                if (System.Enum.TryParse("Joystick" + key_i, false, out Joystick joy))
                {
                    #region All other player controls

                    //PWM controls
                    for (int pwm_i = 0; pwm_i < PWM_COUNT - PWM_OFFSET; pwm_i++)
                    {
                        if (System.Enum.TryParse("Axis" + key_i + "Positive", false, out JoystickAxis axis_pos) &&
                            System.Enum.TryParse("Axis" + key_i + "Negative", false, out JoystickAxis axis_neg))
                        {
                            buttons[player_i].pwmPos[pwm_i] = Players[player_i].SetKey(controlProfile, key_i + ": PWM " + (pwm_i + PWM_OFFSET) + " Positive", new JoystickInput(axis_pos, joy));
                            buttons[player_i].pwmNeg[pwm_i] = Players[player_i].SetKey(controlProfile, key_i + ": PWM " + (pwm_i + PWM_OFFSET) + " Negative", new JoystickInput(axis_neg, joy));
                        }
                        else
                        {
                            throw new System.Exception("Error configuring PWM buttons");
                        }
                    }

                    //Other Controls
                    buttons[player_i].resetRobot = Players[player_i].SetKey(controlProfile, key_i + ": Reset Robot", new JoystickInput(JoystickButton.Button8, joy));
                    buttons[player_i].resetField = Players[player_i].SetKey(controlProfile, key_i + ": Reset Field", new JoystickInput(JoystickButton.Button9, joy));
                    buttons[player_i].replayMode = Players[player_i].SetKey(controlProfile, key_i + ": Replay Mode", new JoystickInput(JoystickButton.Button12, joy));
                    buttons[player_i].cameraToggle = Players[player_i].SetKey(controlProfile, key_i + ": Camera Toggle", new JoystickInput(JoystickButton.Button7, joy));
                    buttons[player_i].scoreboard = Players[player_i].SetKey(controlProfile, key_i + ": Scoreboard", new JoystickInput(JoystickButton.Button10, joy));
                    buttons[player_i].trajectory = Players[player_i].SetKey(controlProfile, key_i + ": Toggle Trajectory", new JoystickInput(JoystickButton.Button11, joy));

                    //Set PWM Axes
                    for(var pwm_i = 0; pwm_i < axes[player_i].pwmAxes.Length; pwm_i++)
                        axes[player_i].pwmAxes[pwm_i] = Players[player_i].SetAxis(controlProfile, key_i + ": PWM " + (pwm_i + PWM_OFFSET) + " Axis " + (pwm_i + PWM_OFFSET), buttons[player_i].pwmNeg[pwm_i], buttons[player_i].pwmNeg[pwm_i]);

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
        /// <returns>True: If user did save their controls
        ///          False: If the user hasn't saved their controls.
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
                    return false;
                }

                lastString = PlayerPrefs.GetString("Controls." + key.name + ".secondary");
                inputString = key.secondaryInput.ToString();

                if (inputString != lastString)
                {
                    return false;
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
            ReadOnlyCollection<KeyMapping> keys = InputControl.GetKeysList();

            foreach (KeyMapping key in keys)
            {
                string inputStr;

                inputStr = PlayerPrefs.GetString("Controls." + key.name + ".primary");

                if (inputStr != "")
                {
                    key.primaryInput = CustomInputFromString(inputStr);
                }

                inputStr = PlayerPrefs.GetString("Controls." + key.name + ".secondary");

                if (inputStr != "")
                {
                    key.secondaryInput = CustomInputFromString(inputStr);
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
            for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
            {
                for (int player_i = 0; player_i < PLAYER_COUNT; player_i++)
                {
                    buttons[player_i].pickup.Clear();
                    buttons[player_i].release.Clear();
                    buttons[player_i].spawnPieces.Clear();
                }

                buttons[0].pickup.Add(Players[0].SetKey(controlProfile, "1: Pick Up " + FieldDataHandler.gamepieces[i].name, KeyCode.LeftControl));
                buttons[0].release.Add(Players[0].SetKey(controlProfile, "1: Release " + FieldDataHandler.gamepieces[i].name, KeyCode.LeftShift));
                buttons[0].spawnPieces.Add(Players[0].SetKey(controlProfile, "1: Spawn " + FieldDataHandler.gamepieces[i].name, KeyCode.RightControl));

                for (int player_i = 1; player_i < PLAYER_COUNT; player_i++)
                {
                    var key_i = player_i + 1;

                    if (System.Enum.TryParse("Joystick" + key_i, false, out Joystick joy))
                    {
                        buttons[player_i].pickup.Add(Players[player_i].SetKey(controlProfile, key_i + ": Pick Up " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button3, joy)));
                        buttons[player_i].release.Add(Players[player_i].SetKey(controlProfile, key_i + ": Release " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button4, joy)));
                        buttons[player_i].spawnPieces.Add(Players[player_i].SetKey(controlProfile, key_i + ": Spawn " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button5, joy)));
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
        private static void ArcadeControls()
        {
            var controlProfile = Player.ControlProfile.ArcadeKeyboard;
            #region Player 1 Controls
            //Basic Controls
            buttons[0].forward = Players[0].SetKey(controlProfile, "1: Forward", KeyCode.UpArrow);
            buttons[0].backward = Players[0].SetKey(controlProfile, "1: Backward", KeyCode.DownArrow);
            buttons[0].left = Players[0].SetKey(controlProfile, "1: Left", KeyCode.LeftArrow);
            buttons[0].right = Players[0].SetKey(controlProfile, "1: Right", KeyCode.RightArrow);

            //Other Controls
            buttons[0].resetRobot = Players[0].SetKey(controlProfile, "1: Reset Robot", KeyCode.R);
            buttons[0].resetField = Players[0].SetKey(controlProfile, "1: Reset Field", KeyCode.F);
            buttons[0].replayMode = Players[0].SetKey(controlProfile, "1: Replay Mode", KeyCode.Tab);
            buttons[0].cameraToggle = Players[0].SetKey(controlProfile, "1: Camera Toggle", KeyCode.C);
            buttons[0].scoreboard = Players[0].SetKey(controlProfile, "1: Scoreboard", KeyCode.Q);
            buttons[0].trajectory = Players[0].SetKey(controlProfile, "1: Toggle Trajectory", KeyCode.T);
            buttons[0].duplicateRobot = Players[0].SetKey(controlProfile, "1: Duplicate Robot", KeyCode.U);
            buttons[0].switchActiveRobot = Players[0].SetKey(controlProfile, "1: Switch Active Robot", KeyCode.Y);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[0].horizontal =  Players[0].SetAxis(controlProfile, "1: Joystick 1 Axis 2", buttons[0].left, buttons[0].right);
            axes[0].vertical =  Players[0].SetAxis(controlProfile, "1: Joystick 1 Axis 4", buttons[0].backward, buttons[0].forward);

            #endregion

            for (int player_i = 1; player_i < PLAYER_COUNT; player_i++)
            {
                var key_i = player_i + 1;

                if (System.Enum.TryParse("Joystick" + key_i, false, out Joystick joy))
                {
                    #region All other player controls

                    //Basic robot controls
                    buttons[player_i].forward = Players[player_i].SetKey(controlProfile, key_i + ": Forward", new JoystickInput(JoystickAxis.Axis2Negative, joy));
                    buttons[player_i].backward = Players[player_i].SetKey(controlProfile, key_i + ": Backward", new JoystickInput(JoystickAxis.Axis2Positive, joy));
                    buttons[player_i].left = Players[player_i].SetKey(controlProfile, key_i + ": Left", new JoystickInput(JoystickAxis.Axis4Negative, joy));
                    buttons[player_i].right = Players[player_i].SetKey(controlProfile, key_i + ": Right", new JoystickInput(JoystickAxis.Axis4Positive, joy));

                    //Set Arcade Drive Axes (PWM [0] and PWM [1])
                    axes[player_i].horizontal = Players[player_i].SetAxis(controlProfile, "Joystick " + key_i + " Axis 2", buttons[player_i].left, buttons[player_i].right);
                    axes[player_i].vertical = Players[player_i].SetAxis(controlProfile, "Joystick " + key_i + " Axis 4", buttons[player_i].backward, buttons[player_i].forward);
                    #endregion
                }
                else
                {
                    throw new System.Exception("Failed to establish joystick index");
                }
            }
            CommonControls(controlProfile);
            UpdateFieldControls(controlProfile);
        }

        /// <summary>
        /// Default settings for TankDrive controls.
        /// Adapted from: https://github.com/Gris87/InputControl
        /// </summary>
        private static void TankControls()
        {
            var controlProfile = Player.ControlProfile.TankJoystick;
            #region Player 1 Controls
            //Tank controls
            buttons[0].tankFrontLeft = Players[0].SetKey(controlProfile, "1: Tank Front Left", new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick1));
            buttons[0].tankBackLeft = Players[0].SetKey(controlProfile, "1: Tank Back Left", new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick1));
            buttons[0].tankFrontRight = Players[0].SetKey(controlProfile, "1: Tank Front Right", new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick1));
            buttons[0].tankBackRight = Players[0].SetKey(controlProfile, "1: Tank Back Right", new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick1));

            //Other Controls
            buttons[0].resetRobot = Players[0].SetKey(controlProfile, "1: Reset Robot", KeyCode.R);
            buttons[0].resetField = Players[0].SetKey(controlProfile, "1: Reset Field", KeyCode.F);
            buttons[0].replayMode = Players[0].SetKey(controlProfile, "1: Replay Mode", KeyCode.Tab);
            buttons[0].cameraToggle = Players[0].SetKey(controlProfile, "1: Camera Toggle", KeyCode.C);
            buttons[0].scoreboard = Players[0].SetKey(controlProfile, "1: Scoreboard", KeyCode.Q);
            buttons[0].trajectory = Players[0].SetKey(controlProfile, "1: Toggle Trajectory", KeyCode.T);
            buttons[0].duplicateRobot = Players[0].SetKey(controlProfile, "1: Duplicate Robot", KeyCode.U);
            buttons[0].switchActiveRobot = Players[0].SetKey(controlProfile, "1: Switch Active Robot", KeyCode.Y);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[0].tankLeftAxes =  Players[0].SetAxis(controlProfile, "1: Joystick 1 Axis 9", buttons[0].tankBackLeft, buttons[0].tankFrontLeft);
            axes[0].tankRightAxes =  Players[0].SetAxis(controlProfile, "1: Joystick 1 Axis 10", buttons[0].tankFrontRight, buttons[0].tankBackRight);

            #endregion

            for (int player_i = 1; player_i < PLAYER_COUNT; player_i++)
            {
                var key_i = player_i + 1;

                if (System.Enum.TryParse("Joystick" + key_i, false, out Joystick joy))
                {
                    #region All Other Players' Controls
                    //Tank Controls
                    buttons[player_i].tankFrontLeft = Players[player_i].SetKey(controlProfile, key_i + ": Tank Front Left", new JoystickInput(JoystickAxis.Axis2Negative, joy));
                    buttons[player_i].tankBackLeft = Players[player_i].SetKey(controlProfile, key_i + ": Tank Back Left", new JoystickInput(JoystickAxis.Axis2Positive, joy));
                    buttons[player_i].tankFrontRight = Players[player_i].SetKey(controlProfile, key_i + ": Tank Front Right", new JoystickInput(JoystickAxis.Axis5Negative, joy));
                    buttons[player_i].tankBackRight = Players[player_i].SetKey(controlProfile, key_i + ": Tank Back Right", new JoystickInput(JoystickAxis.Axis5Positive, joy));

                    //Set Tank Drive Axes (PWM [0] and PWM [1])
                    axes[player_i].tankLeftAxes = Players[player_i].SetAxis(controlProfile, "Joystick " + key_i + " Axis 9", buttons[player_i].tankBackLeft, buttons[player_i].tankFrontLeft);
                    axes[player_i].tankRightAxes = Players[player_i].SetAxis(controlProfile, "Joystick " + key_i + " Axis 10", buttons[player_i].tankFrontRight, buttons[player_i].tankBackRight);
                    #endregion
                }
                else
                {
                    throw new System.Exception("Failed to establish joystick index");
                }
            }
            CommonControls(controlProfile);
            UpdateFieldControls(controlProfile);
        }
    }
}