using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Synthesis.Input.Inputs;
using Synthesis.Input.Enums;

namespace Synthesis.Input
{
    public class Controls
    {
        #region Old Controls: 2017 and Older
        //public enum Control
        //{
        //    Forward, Backward, Right, Left, ResetRobot, CameraToggle, pwm2Plus, pwm2Neg, pwm3Plus, pwm3Neg, pwm4Plus,
        //    pwm4Neg, pwm5Plus, pwm5Neg, pwm6Plus, pwm6Neg, PickupPrimary, ReleasePrimary, SpawnPrimary, PickupSecondary,
        //    ReleaseSecondary, SpawnSecondary
        //};

        //public static KeyCode[] ControlKey = new KeyCode[23];
        //public static KeyCode[] BackupKeys = new KeyCode[23];
        //public static readonly string[] ControlName = { "Move Forward", "Move Backward", "Turn Right", "Turn Left", "Reset Robot",
        //                                                "Toggle Camera", "PWM 2 Positive", "PWM 2 Negative", "PWM 3 Positive",
        //                                                "PWM 3 Negative", "PWM 4 Positive", "PWM 4 Negative", "PWM 5 Positive",
        //                                                "PWM 5 Negative", "PWM 6 Positive", "PWM 6 Negative", "Pick Up Primary Gamepiece",
        //                                                "Release Primary Gamepiece", "Spawn Primary Gamepiece", "Pick Up Secondary Gamepiece",
        //                                                "Release Secondary Gamepiece", "Spawn Secondary Gamepiece"
        //};
        #endregion

        public static bool TankDriveEnabled;    //Checks if TankDrive is enabled

        ///Player indexes (for initializing and creating separate player key lists) <see cref="InputControl"/>
        public static int PlayerOneIndex = 0;
        public static int PlayerTwoIndex = 1;
        public static int PlayerThreeIndex = 2;
        public static int PlayerFourIndex = 3;
        public static int PlayerFiveIndex = 4;
        public static int PlayerSixIndex = 5;

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
            //public KeyMapping replayMode;
            public KeyMapping pickupPrimary;
            public KeyMapping releasePrimary;
            public KeyMapping spawnPrimary;
            public KeyMapping pickupSecondary;
            public KeyMapping releaseSecondary;
            public KeyMapping spawnSecondary;
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
            TankControls();
            ArcadeControls();
        }

        public static void Init()
        {
            // Nothing. It just call static constructor if needed
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
            ReadOnlyCollection<KeyMapping> inputKeys = InputControl.GetActivePlayerKeys();

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
        /// Sets controls to Arcade Drive.
        /// </summary>
        public static void ArcadeDrive()
        {
            ArcadeControls();
            TankDriveEnabled = false;

            if (GameObject.Find("SettingsMode") != null)
            {
                //Loads initialized keys then defaults to PlayerOne's list on the landing page
                Load();
                GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerOneButtons();
                GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
            }
        }

        /// <summary>
        /// Resets ALL player controls to default arcade drive controls.
        /// 2017: NOT IN USE; Reserve for possible use (see ArcadeDrive())
        /// </summary>
        public static void ResetArcadeDrive()
        {
            ArcadeControls();
            TankDriveEnabled = false;

            if (GameObject.Find("SettingsMode") != null)
            {
                //Updates the controls text (keybuttons)
                GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
            }
        }

        /// <summary>
        /// Sets controls to Tank Drive.
        /// 2017: NOT IN USE; Reserve for possible use (see ArcadeDrive())
        /// </summary>
        public static void TankDrive()
        {
            TankControls();
            TankDriveEnabled = true;

            if (GameObject.Find("SettingsMode") != null)
            {
                //Loads initialized keys then defaults to PlayerOne's list on the landing page
                Load();
                GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerOneButtons();
                GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
            }
        }

        /// <summary>
        /// Resets ALL player controls to default tank drive controls.
        /// 2017: NOT IN USE; Reserve for possible use (see ArcadeDrive())
        /// </summary>
        public static void ResetTankDrive()
        {
            TankControls();
            TankDriveEnabled = true;

            if (GameObject.Find("SettingsMode") != null)
            {
                //Updates the controls text (keybuttons)
                GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
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

        /// <summary>
        /// Default settings for ArcadeDrive controls.
        /// Adapted from: https://github.com/Gris87/InputControl
        /// </summary>
        public static void ArcadeControls()
        {
            // TODO: Please someone simplify this. Load the defaults from an Json file or something.

            #region Primary Controls
            //Basic Controls
            buttons[0].forward = InputControl.SetKey("1: Forward", PlayerOneIndex, KeyCode.UpArrow, false);
            buttons[0].backward = InputControl.SetKey("1: Backward", PlayerOneIndex, KeyCode.DownArrow, false);
            buttons[0].left = InputControl.SetKey("1: Left", PlayerOneIndex, KeyCode.LeftArrow, false);
            buttons[0].right = InputControl.SetKey("1: Right", PlayerOneIndex, KeyCode.RightArrow, false);

            //Remaining PWM controls
            buttons[0].pwm2Plus = InputControl.SetKey("1: PWM 2 Positive", PlayerOneIndex, KeyCode.Alpha1, false);
            buttons[0].pwm2Neg = InputControl.SetKey("1: PWM 2 Negative", PlayerOneIndex, KeyCode.Alpha2, false);
            buttons[0].pwm3Plus = InputControl.SetKey("1: PWM 3 Positive", PlayerOneIndex, KeyCode.Alpha3, false);
            buttons[0].pwm3Neg = InputControl.SetKey("1: PWM 3 Negative", PlayerOneIndex, KeyCode.Alpha4, false);
            buttons[0].pwm4Plus = InputControl.SetKey("1: PWM 4 Positive", PlayerOneIndex, KeyCode.Alpha5, false);
            buttons[0].pwm4Neg = InputControl.SetKey("1: PWM 4 Negative", PlayerOneIndex, KeyCode.Alpha6, false);
            buttons[0].pwm5Plus = InputControl.SetKey("1: PWM 5 Positive", PlayerOneIndex, KeyCode.Alpha7, false);
            buttons[0].pwm5Neg = InputControl.SetKey("1: PWM 5 Negative", PlayerOneIndex, KeyCode.Alpha8, false);
            buttons[0].pwm6Plus = InputControl.SetKey("1: PWM 6 Positive", PlayerOneIndex, KeyCode.Alpha9, false);
            buttons[0].pwm6Neg = InputControl.SetKey("1: PWM 6 Negative", PlayerOneIndex, KeyCode.Alpha0, false);
            buttons[0].pwm7Plus = InputControl.SetKey("1: PWM 7 Positive", PlayerOneIndex, KeyCode.Slash, false);
            buttons[0].pwm7Neg = InputControl.SetKey("1: PWM 7 Negative", PlayerOneIndex, KeyCode.Period, false);
            buttons[0].pwm8Plus = InputControl.SetKey("1: PWM 8 Positive", PlayerOneIndex, KeyCode.Comma, false);
            buttons[0].pwm8Neg = InputControl.SetKey("1: PWM 8 Negative", PlayerOneIndex, KeyCode.M, false);
            buttons[0].pwm9Plus = InputControl.SetKey("1: PWM 9 Positive", PlayerOneIndex, KeyCode.N, false);
            buttons[0].pwm9Neg = InputControl.SetKey("1: PWM 9 Negative", PlayerOneIndex, KeyCode.B, false);

            //Other Controls
            buttons[0].resetRobot = InputControl.SetKey("1: Reset Robot", PlayerOneIndex, KeyCode.R, false);
            buttons[0].resetField = InputControl.SetKey("1: Reset Field", PlayerOneIndex, KeyCode.F, false);
            buttons[0].cameraToggle = InputControl.SetKey("1: Camera Toggle", PlayerOneIndex, KeyCode.C, false);
            buttons[0].pickupPrimary = InputControl.SetKey("1: Pick Up Primary Gamepiece", PlayerOneIndex, KeyCode.LeftControl, false);
            buttons[0].releasePrimary = InputControl.SetKey("1: Release Primary Gamepiece", PlayerOneIndex, KeyCode.LeftShift, false);
            buttons[0].spawnPrimary = InputControl.SetKey("1: Spawn Primary Gamepiece", PlayerOneIndex, KeyCode.LeftBracket, false);
            buttons[0].pickupSecondary = InputControl.SetKey("1: Pick Up Secondary Gamepiece", PlayerOneIndex, KeyCode.LeftControl, false);
            buttons[0].releaseSecondary = InputControl.SetKey("1: Release Secondary Gamepiece", PlayerOneIndex, KeyCode.LeftShift, false);
            buttons[0].spawnSecondary = InputControl.SetKey("1: Spawn Secondary Gamepiece", PlayerOneIndex, KeyCode.RightBracket, false);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[0].horizontal = InputControl.SetAxis("1: Joystick 1 Axis 2", PlayerOneIndex, buttons[0].left, buttons[0].right, false);
            axes[0].vertical = InputControl.SetAxis("1: Joystick 1 Axis 4", PlayerOneIndex, buttons[0].backward, buttons[0].forward, false);

            //Set PWM Axes
            axes[0].pwm2Axes = InputControl.SetAxis("1: PWM 2 Axis 3", PlayerOneIndex, buttons[0].pwm2Neg, buttons[0].pwm2Plus, false);
            axes[0].pwm3Axes = InputControl.SetAxis("1: PWM 3 Axis 5", PlayerOneIndex, buttons[0].pwm3Neg, buttons[0].pwm3Plus, false);
            axes[0].pwm4Axes = InputControl.SetAxis("1: PWM 4 Axis 6", PlayerOneIndex, buttons[0].pwm4Neg, buttons[0].pwm4Plus, false);
            axes[0].pwm5Axes = InputControl.SetAxis("1: PWM 5 Axis 7", PlayerOneIndex, buttons[0].pwm5Neg, buttons[0].pwm5Plus, false);
            axes[0].pwm6Axes = InputControl.SetAxis("1: PWM 6 Axis 8", PlayerOneIndex, buttons[0].pwm6Neg, buttons[0].pwm6Plus, false);
            axes[0].pwm7Axes = InputControl.SetAxis("1: PWM 7 Axis 9", PlayerOneIndex, buttons[0].pwm7Neg, buttons[0].pwm7Plus, false);
            axes[0].pwm8Axes = InputControl.SetAxis("1: PWM 8 Axis 10", PlayerOneIndex, buttons[0].pwm8Neg, buttons[0].pwm8Plus, false);
            axes[0].pwm9Axes = InputControl.SetAxis("1: PWM 9 Axis 11", PlayerOneIndex, buttons[0].pwm9Neg, buttons[0].pwm9Plus, false);
            #endregion

            #region Player 2 Controls
            //Basic robot controls
            buttons[1].forward = InputControl.SetKey("2: Forward", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick2), false);
            buttons[1].backward = InputControl.SetKey("2: Backward", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick2), false);
            buttons[1].left = InputControl.SetKey("2: Left", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick2), false);
            buttons[1].right = InputControl.SetKey("2: Right", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick2), false);

            //Remaining PWM controls
            buttons[1].pwm2Plus = InputControl.SetKey("2: PWM 2 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick2), false);
            buttons[1].pwm2Neg = InputControl.SetKey("2: PWM 2 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick2), false);
            buttons[1].pwm3Plus = InputControl.SetKey("2: PWM 3 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick2), false);
            buttons[1].pwm3Neg = InputControl.SetKey("2: PWM 3 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick2), false);
            buttons[1].pwm4Plus = InputControl.SetKey("2: PWM 4 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick2), false);
            buttons[1].pwm4Neg = InputControl.SetKey("2: PWM 4 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick2), false);
            buttons[1].pwm5Plus = InputControl.SetKey("2: PWM 5 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick2), false);
            buttons[1].pwm5Neg = InputControl.SetKey("2: PWM 5 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick2), false);
            buttons[1].pwm6Plus = InputControl.SetKey("2: PWM 6 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick2), false);
            buttons[1].pwm6Neg = InputControl.SetKey("2: PWM 6 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick2), false);
            buttons[1].pwm7Plus = InputControl.SetKey("2: PWM 7 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick2), false);
            buttons[1].pwm7Neg = InputControl.SetKey("2: PWM 7 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick2), false);
            buttons[1].pwm8Plus = InputControl.SetKey("2: PWM 8 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick2), false);
            buttons[1].pwm8Neg = InputControl.SetKey("2: PWM 8 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick2), false);
            buttons[1].pwm9Plus = InputControl.SetKey("2: PWM 9 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis11Positive, Joystick.Joystick2), false);
            buttons[1].pwm9Neg = InputControl.SetKey("2: PWM 9 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis11Negative, Joystick.Joystick2), false);

            //Other Controls
            buttons[1].resetRobot = InputControl.SetKey("2: Reset Robot", PlayerTwoIndex, new JoystickInput(JoystickButton.Button8, Joystick.Joystick2), false);
            buttons[1].resetField = InputControl.SetKey("2: Reset Field", PlayerTwoIndex, new JoystickInput(JoystickButton.Button9, Joystick.Joystick2), false);
            //buttons[1].replayMode = InputControl.setKey("2: Replay Mode", PlayerTwoIndex, KeyCode.Tab, new JoystickInput(JoystickButton.Button6, Joystick.Joystick1), false);
            buttons[1].pickupPrimary = InputControl.SetKey("2: Pick Up Primary Gamepiece", PlayerTwoIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick2), false);
            buttons[1].releasePrimary = InputControl.SetKey("2: Release Primary Gamepiece", PlayerTwoIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick2), false);
            buttons[1].spawnPrimary = InputControl.SetKey("2: Spawn Primary Gamepiece", PlayerTwoIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick2), false);
            buttons[1].pickupSecondary = InputControl.SetKey("2: Pick Up Secondary Gamepiece", PlayerTwoIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick2), false);
            buttons[1].releaseSecondary = InputControl.SetKey("2: Release Secondary Gamepiece", PlayerTwoIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick2), false);
            buttons[1].spawnSecondary = InputControl.SetKey("2: Spawn Secondary Gamepiece", PlayerTwoIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick2), false);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[1].horizontal = InputControl.SetAxis("Joystick 2 Axis 2", PlayerTwoIndex, buttons[1].left, buttons[1].right, false);
            axes[1].vertical = InputControl.SetAxis("Joystick 2 Axis 4", PlayerTwoIndex, buttons[1].backward, buttons[1].forward, false);

            //Set PWM Axes
            axes[1].pwm2Axes = InputControl.SetAxis("2: PWM 2 Axis 3", PlayerOneIndex, buttons[1].pwm2Neg, buttons[1].pwm2Plus, false);
            axes[1].pwm3Axes = InputControl.SetAxis("2: PWM 3 Axis 5", PlayerOneIndex, buttons[1].pwm3Neg, buttons[1].pwm3Plus, false);
            axes[1].pwm4Axes = InputControl.SetAxis("2: PWM 4 Axis 6", PlayerOneIndex, buttons[1].pwm4Neg, buttons[1].pwm4Plus, false);
            axes[1].pwm5Axes = InputControl.SetAxis("2: PWM 5 Axis 7", PlayerOneIndex, buttons[1].pwm5Neg, buttons[1].pwm5Plus, false);
            axes[1].pwm6Axes = InputControl.SetAxis("2: PWM 6 Axis 8", PlayerOneIndex, buttons[1].pwm6Neg, buttons[1].pwm6Plus, false);
            axes[1].pwm7Axes = InputControl.SetAxis("2: PWM 7 Axis 9", PlayerOneIndex, buttons[1].pwm7Neg, buttons[1].pwm7Plus, false);
            axes[1].pwm8Axes = InputControl.SetAxis("2: PWM 8 Axis 10", PlayerOneIndex, buttons[1].pwm8Neg, buttons[1].pwm8Plus, false);
            axes[1].pwm9Axes = InputControl.SetAxis("2: PWM 9 Axis 11", PlayerOneIndex, buttons[1].pwm9Neg, buttons[1].pwm9Plus, false);
            #endregion

            #region Player 3 Controls
            //Basic robot controls
            buttons[2].forward = InputControl.SetKey("3: Forward", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick3), false);
            buttons[2].backward = InputControl.SetKey("3: Backward", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick3), false);
            buttons[2].left = InputControl.SetKey("3: Left", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick3), false);
            buttons[2].right = InputControl.SetKey("3: Right", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick3), false);

            //Remaining PWM controls
            buttons[2].pwm2Plus = InputControl.SetKey("3: PWM 2 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick3), false);
            buttons[2].pwm2Neg = InputControl.SetKey("3: PWM 2 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick3), false);
            buttons[2].pwm3Plus = InputControl.SetKey("3: PWM 3 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick3), false);
            buttons[2].pwm3Neg = InputControl.SetKey("3: PWM 3 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick3), false);
            buttons[2].pwm4Plus = InputControl.SetKey("3: PWM 4 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick3), false);
            buttons[2].pwm4Neg = InputControl.SetKey("3: PWM 4 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick3), false);
            buttons[2].pwm5Plus = InputControl.SetKey("3: PWM 5 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick3), false);
            buttons[2].pwm5Neg = InputControl.SetKey("3: PWM 5 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick3), false);
            buttons[2].pwm6Plus = InputControl.SetKey("3: PWM 6 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick3), false);
            buttons[2].pwm6Neg = InputControl.SetKey("3: PWM 6 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick3), false);
            buttons[2].pwm7Plus = InputControl.SetKey("3: PWM 7 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick3), false);
            buttons[2].pwm7Neg = InputControl.SetKey("3: PWM 7 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick3), false);
            buttons[2].pwm8Plus = InputControl.SetKey("3: PWM 8 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick3), false);
            buttons[2].pwm8Neg = InputControl.SetKey("3: PWM 8 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick3), false);
            buttons[2].pwm9Plus = InputControl.SetKey("3: PWM 9 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis11Positive, Joystick.Joystick3), false);
            buttons[2].pwm9Neg = InputControl.SetKey("3: PWM 9 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis11Negative, Joystick.Joystick3), false);

            //Other Controls
            buttons[2].resetRobot = InputControl.SetKey("3: Reset Robot", PlayerThreeIndex, new JoystickInput(JoystickButton.Button8, Joystick.Joystick3), false);
            buttons[2].resetField = InputControl.SetKey("3: Reset Field", PlayerThreeIndex, new JoystickInput(JoystickButton.Button9, Joystick.Joystick3), false);
            //buttons[2].replayMode = InputControl.setKey("3: Replay Mode", PlayerThreeIndex, KeyCode.Tab, new JoystickInput(JoystickButton.Button6, Joystick.Joystick1), false);
            buttons[2].pickupPrimary = InputControl.SetKey("3: Pick Up Primary Gamepiece", PlayerThreeIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick3), false);
            buttons[2].releasePrimary = InputControl.SetKey("3: Release Primary Gamepiece", PlayerThreeIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick3), false);
            buttons[2].spawnPrimary = InputControl.SetKey("3: Spawn Primary Gamepiece", PlayerThreeIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick3), false);
            buttons[2].pickupSecondary = InputControl.SetKey("3: Pick Up Secondary Gamepiece", PlayerThreeIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick3), false);
            buttons[2].releaseSecondary = InputControl.SetKey("3: Release Secondary Gamepiece", PlayerThreeIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick3), false);
            buttons[2].spawnSecondary = InputControl.SetKey("3: Spawn Secondary Gamepiece", PlayerThreeIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick3), false);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[2].horizontal = InputControl.SetAxis("Joystick 3 Axis 2", PlayerThreeIndex, buttons[2].left, buttons[2].right, false);
            axes[2].vertical = InputControl.SetAxis("Joystick 3 Axis 4", PlayerThreeIndex, buttons[2].backward, buttons[2].forward, false);

            //Set PWM Axes
            axes[2].pwm2Axes = InputControl.SetAxis("3: PWM 2 Axis 3", PlayerOneIndex, buttons[2].pwm2Neg, buttons[2].pwm2Plus, false);
            axes[2].pwm3Axes = InputControl.SetAxis("3: PWM 3 Axis 5", PlayerOneIndex, buttons[2].pwm3Neg, buttons[2].pwm3Plus, false);
            axes[2].pwm4Axes = InputControl.SetAxis("3: PWM 4 Axis 6", PlayerOneIndex, buttons[2].pwm4Neg, buttons[2].pwm4Plus, false);
            axes[2].pwm5Axes = InputControl.SetAxis("3: PWM 5 Axis 7", PlayerOneIndex, buttons[2].pwm5Neg, buttons[2].pwm5Plus, false);
            axes[2].pwm6Axes = InputControl.SetAxis("3: PWM 6 Axis 8", PlayerOneIndex, buttons[2].pwm6Neg, buttons[2].pwm6Plus, false);
            axes[2].pwm7Axes = InputControl.SetAxis("3: PWM 7 Axis 9", PlayerOneIndex, buttons[2].pwm7Neg, buttons[2].pwm7Plus, false);
            axes[2].pwm8Axes = InputControl.SetAxis("3: PWM 8 Axis 10", PlayerOneIndex, buttons[2].pwm8Neg, buttons[2].pwm8Plus, false);
            axes[2].pwm9Axes = InputControl.SetAxis("3: PWM 9 Axis 11", PlayerOneIndex, buttons[2].pwm9Neg, buttons[2].pwm9Plus, false);
            #endregion

            #region Player 4 Controls
            //Basic robot controls
            buttons[3].forward = InputControl.SetKey("4: Forward", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick4), false);
            buttons[3].backward = InputControl.SetKey("4: Backward", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick4), false);
            buttons[3].left = InputControl.SetKey("4: Left", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick4), false);
            buttons[3].right = InputControl.SetKey("4: Right", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick4), false);

            //Remaining PWM controls
            buttons[3].pwm2Plus = InputControl.SetKey("4: PWM 2 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick4), false);
            buttons[3].pwm2Neg = InputControl.SetKey("4: PWM 2 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick4), false);
            buttons[3].pwm3Plus = InputControl.SetKey("4: PWM 3 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick4), false);
            buttons[3].pwm3Neg = InputControl.SetKey("4: PWM 3 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick4), false);
            buttons[3].pwm4Plus = InputControl.SetKey("4: PWM 4 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick4), false);
            buttons[3].pwm4Neg = InputControl.SetKey("4: PWM 4 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick4), false);
            buttons[3].pwm5Plus = InputControl.SetKey("4: PWM 5 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick4), false);
            buttons[3].pwm5Neg = InputControl.SetKey("4: PWM 5 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick4), false);
            buttons[3].pwm6Plus = InputControl.SetKey("4: PWM 6 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick4), false);
            buttons[3].pwm6Neg = InputControl.SetKey("4: PWM 6 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick4), false);
            buttons[3].pwm7Plus = InputControl.SetKey("4: PWM 7 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick4), false);
            buttons[3].pwm7Neg = InputControl.SetKey("4: PWM 7 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick4), false);
            buttons[3].pwm8Plus = InputControl.SetKey("4: PWM 8 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick4), false);
            buttons[3].pwm8Neg = InputControl.SetKey("4: PWM 8 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick4), false);
            buttons[3].pwm9Plus = InputControl.SetKey("4: PWM 9 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis11Positive, Joystick.Joystick4), false);
            buttons[3].pwm9Neg = InputControl.SetKey("4: PWM 9 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis11Negative, Joystick.Joystick4), false);

            //Other Controls
            buttons[3].resetRobot = InputControl.SetKey("4: Reset Robot", PlayerFourIndex, new JoystickInput(JoystickButton.Button8, Joystick.Joystick4), false);
            buttons[3].resetField = InputControl.SetKey("4: Reset Field", PlayerFourIndex, new JoystickInput(JoystickButton.Button9, Joystick.Joystick4), false);
            //buttons[3].replayMode = InputControl.setKey("4: Replay Mode", PlayerFourIndex, KeyCode.Tab, new JoystickInput(JoystickButton.Button6, Joystick.Joystick1), false);
            buttons[3].pickupPrimary = InputControl.SetKey("4: Pick Up Primary Gamepiece", PlayerFourIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick4), false);
            buttons[3].releasePrimary = InputControl.SetKey("4: Release Primary Gamepiece", PlayerFourIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick4), false);
            buttons[3].spawnPrimary = InputControl.SetKey("4: Spawn Primary Gamepiece", PlayerFourIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick4), false);
            buttons[3].pickupSecondary = InputControl.SetKey("4: Pick Up Secondary Gamepiece", PlayerFourIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick4), false);
            buttons[3].releaseSecondary = InputControl.SetKey("4: Release Secondary Gamepiece", PlayerFourIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick4), false);
            buttons[3].spawnSecondary = InputControl.SetKey("4: Spawn Secondary Gamepiece", PlayerFourIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick4), false);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[3].horizontal = InputControl.SetAxis("Joystick 4 Axis 2", PlayerFourIndex, buttons[3].left, buttons[3].right, false);
            axes[3].vertical = InputControl.SetAxis("Joystick 4 Axis 4", PlayerFourIndex, buttons[3].backward, buttons[3].forward, false);

            //Set PWM Axes
            axes[3].pwm2Axes = InputControl.SetAxis("4: PWM 2 Axis 3", PlayerOneIndex, buttons[3].pwm2Neg, buttons[3].pwm2Plus, false);
            axes[3].pwm3Axes = InputControl.SetAxis("4: PWM 3 Axis 5", PlayerOneIndex, buttons[3].pwm3Neg, buttons[3].pwm3Plus, false);
            axes[3].pwm4Axes = InputControl.SetAxis("4: PWM 4 Axis 6", PlayerOneIndex, buttons[3].pwm4Neg, buttons[3].pwm4Plus, false);
            axes[3].pwm5Axes = InputControl.SetAxis("4: PWM 5 Axis 7", PlayerOneIndex, buttons[3].pwm5Neg, buttons[3].pwm5Plus, false);
            axes[3].pwm6Axes = InputControl.SetAxis("4: PWM 6 Axis 8", PlayerOneIndex, buttons[3].pwm6Neg, buttons[3].pwm6Plus, false);
            axes[3].pwm7Axes = InputControl.SetAxis("4: PWM 7 Axis 9", PlayerOneIndex, buttons[3].pwm7Neg, buttons[3].pwm7Plus, false);
            axes[3].pwm8Axes = InputControl.SetAxis("4: PWM 8 Axis 10", PlayerOneIndex, buttons[3].pwm8Neg, buttons[3].pwm8Plus, false);
            axes[3].pwm9Axes = InputControl.SetAxis("4: PWM 9 Axis 11", PlayerOneIndex, buttons[3].pwm9Neg, buttons[3].pwm9Plus, false);
            #endregion

            #region Player 5 Controls
            //Basic robot controls
            buttons[4].forward = InputControl.SetKey("5: Forward", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick5), false);
            buttons[4].backward = InputControl.SetKey("5: Backward", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick5), false);
            buttons[4].left = InputControl.SetKey("5: Left", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick5), false);
            buttons[4].right = InputControl.SetKey("5: Right", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick5), false);

            //Remaining PWM controls
            buttons[4].pwm2Plus = InputControl.SetKey("5: PWM 2 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick5), false);
            buttons[4].pwm2Neg = InputControl.SetKey("5: PWM 2 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick5), false);
            buttons[4].pwm3Plus = InputControl.SetKey("5: PWM 3 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick5), false);
            buttons[4].pwm3Neg = InputControl.SetKey("5: PWM 3 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick5), false);
            buttons[4].pwm4Plus = InputControl.SetKey("5: PWM 4 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick5), false);
            buttons[4].pwm4Neg = InputControl.SetKey("5: PWM 4 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick5), false);
            buttons[4].pwm5Plus = InputControl.SetKey("5: PWM 5 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick5), false);
            buttons[4].pwm5Neg = InputControl.SetKey("5: PWM 5 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick5), false);
            buttons[4].pwm6Plus = InputControl.SetKey("5: PWM 6 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick5), false);
            buttons[4].pwm6Neg = InputControl.SetKey("5: PWM 6 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick5), false);
            buttons[4].pwm7Plus = InputControl.SetKey("5: PWM 7 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick5), false);
            buttons[4].pwm7Neg = InputControl.SetKey("5: PWM 7 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick5), false);
            buttons[4].pwm8Plus = InputControl.SetKey("5: PWM 8 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick5), false);
            buttons[4].pwm8Neg = InputControl.SetKey("5: PWM 8 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick5), false);
            buttons[4].pwm9Plus = InputControl.SetKey("5: PWM 9 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis11Positive, Joystick.Joystick5), false);
            buttons[4].pwm9Neg = InputControl.SetKey("5: PWM 9 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis11Negative, Joystick.Joystick5), false);

            //Other Controls
            buttons[4].resetRobot = InputControl.SetKey("5: Reset Robot", PlayerFiveIndex, new JoystickInput(JoystickButton.Button8, Joystick.Joystick5), false);
            buttons[4].resetField = InputControl.SetKey("5: Reset Field", PlayerFiveIndex, new JoystickInput(JoystickButton.Button9, Joystick.Joystick5), false);
            //buttons[4].replayMode = InputControl.setKey("5: Replay Mode", PlayerFiveIndex, KeyCode.Tab, new JoystickInput(JoystickButton.Button6, Joystick.Joystick1), false);
            buttons[4].pickupPrimary = InputControl.SetKey("5: Pick Up Primary Gamepiece", PlayerFiveIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick5), false);
            buttons[4].releasePrimary = InputControl.SetKey("5: Release Primary Gamepiece", PlayerFiveIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick5), false);
            buttons[4].spawnPrimary = InputControl.SetKey("5: Spawn Primary Gamepiece", PlayerFiveIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick5), false);
            buttons[4].pickupSecondary = InputControl.SetKey("5: Pick Up Secondary Gamepiece", PlayerFiveIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick5), false);
            buttons[4].releaseSecondary = InputControl.SetKey("5: Release Secondary Gamepiece", PlayerFiveIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick5), false);
            buttons[4].spawnSecondary = InputControl.SetKey("5: Spawn Secondary Gamepiece", PlayerFiveIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick5), false);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[4].horizontal = InputControl.SetAxis("Joystick 5 Axis 2", PlayerFiveIndex, buttons[4].left, buttons[4].right, false);
            axes[4].vertical = InputControl.SetAxis("Joystick 5 Axis 4", PlayerFiveIndex, buttons[4].backward, buttons[4].forward, false);

            //Set PWM Axes
            axes[4].pwm2Axes = InputControl.SetAxis("5: PWM 2 Axis 3", PlayerOneIndex, buttons[4].pwm2Neg, buttons[4].pwm2Plus, false);
            axes[4].pwm3Axes = InputControl.SetAxis("5: PWM 3 Axis 5", PlayerOneIndex, buttons[4].pwm3Neg, buttons[4].pwm3Plus, false);
            axes[4].pwm4Axes = InputControl.SetAxis("5: PWM 4 Axis 6", PlayerOneIndex, buttons[4].pwm4Neg, buttons[4].pwm4Plus, false);
            axes[4].pwm5Axes = InputControl.SetAxis("5: PWM 5 Axis 7", PlayerOneIndex, buttons[4].pwm5Neg, buttons[4].pwm5Plus, false);
            axes[4].pwm6Axes = InputControl.SetAxis("5: PWM 6 Axis 8", PlayerOneIndex, buttons[4].pwm6Neg, buttons[4].pwm6Plus, false);
            axes[4].pwm7Axes = InputControl.SetAxis("5: PWM 7 Axis 9", PlayerOneIndex, buttons[4].pwm7Neg, buttons[4].pwm7Plus, false);
            axes[4].pwm8Axes = InputControl.SetAxis("5: PWM 8 Axis 10", PlayerOneIndex, buttons[4].pwm8Neg, buttons[4].pwm8Plus, false);
            axes[4].pwm9Axes = InputControl.SetAxis("5: PWM 9 Axis 11", PlayerOneIndex, buttons[4].pwm9Neg, buttons[4].pwm9Plus, false);
            #endregion

            #region Player 6 Controls
            //Basic robot controls
            buttons[5].forward = InputControl.SetKey("6: Forward", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick6), false);
            buttons[5].backward = InputControl.SetKey("6: Backward", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick6), false);
            buttons[5].left = InputControl.SetKey("6: Left", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick6), false);
            buttons[5].right = InputControl.SetKey("6: Right", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick6), false);

            //Remaining PWM controls
            buttons[5].pwm2Plus = InputControl.SetKey("6: PWM 2 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick6), false);
            buttons[5].pwm2Neg = InputControl.SetKey("6: PWM 2 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick6), false);
            buttons[5].pwm3Plus = InputControl.SetKey("6: PWM 3 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick6), false);
            buttons[5].pwm3Neg = InputControl.SetKey("6: PWM 3 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick6), false);
            buttons[5].pwm4Plus = InputControl.SetKey("6: PWM 4 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick6), false);
            buttons[5].pwm4Neg = InputControl.SetKey("6: PWM 4 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick6), false);
            buttons[5].pwm5Plus = InputControl.SetKey("6: PWM 5 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick6), false);
            buttons[5].pwm5Neg = InputControl.SetKey("6: PWM 5 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick6), false);
            buttons[5].pwm6Plus = InputControl.SetKey("6: PWM 6 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick6), false);
            buttons[5].pwm6Neg = InputControl.SetKey("6: PWM 6 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick6), false);
            buttons[5].pwm7Plus = InputControl.SetKey("6: PWM 7 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick6), false);
            buttons[5].pwm7Neg = InputControl.SetKey("6: PWM 7 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick6), false);
            buttons[5].pwm8Plus = InputControl.SetKey("6: PWM 8 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick6), false);
            buttons[5].pwm8Neg = InputControl.SetKey("6: PWM 8 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick6), false);
            buttons[5].pwm9Plus = InputControl.SetKey("6: PWM 9 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis11Positive, Joystick.Joystick6), false);
            buttons[5].pwm9Neg = InputControl.SetKey("6: PWM 9 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis11Negative, Joystick.Joystick6), false);

            //Other Controls
            buttons[5].resetRobot = InputControl.SetKey("6: Reset Robot", PlayerSixIndex, new JoystickInput(JoystickButton.Button8, Joystick.Joystick6), false);
            buttons[5].resetField = InputControl.SetKey("6: Reset Field", PlayerSixIndex, new JoystickInput(JoystickButton.Button9, Joystick.Joystick6), false);
            //buttons[5].replayMode = InputControl.setKey("6: Replay Mode", PlayerSixIndex, KeyCode.Tab, new JoystickInput(JoystickButton.Button6, Joystick.Joystick1), false);
            buttons[5].pickupPrimary = InputControl.SetKey("6: Pick Up Primary Gamepiece", PlayerSixIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick6), false);
            buttons[5].releasePrimary = InputControl.SetKey("6: Release Primary Gamepiece", PlayerSixIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick6), false);
            buttons[5].spawnPrimary = InputControl.SetKey("6: Spawn Primary Gamepiece", PlayerSixIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick6), false);
            buttons[5].pickupSecondary = InputControl.SetKey("6: Pick Up Secondary Gamepiece", PlayerSixIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick6), false);
            buttons[5].releaseSecondary = InputControl.SetKey("6: Release Secondary Gamepiece", PlayerSixIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick6), false);
            buttons[5].spawnSecondary = InputControl.SetKey("6: Spawn Secondary Gamepiece", PlayerSixIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick6), false);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[5].horizontal = InputControl.SetAxis("Joystick 6 Axis 2", PlayerSixIndex, buttons[5].left, buttons[5].right, false);
            axes[5].vertical = InputControl.SetAxis("Joystick 6 Axis 4", PlayerSixIndex, buttons[5].backward, buttons[5].forward, false);

            //Set PWM Axes
            axes[5].pwm2Axes = InputControl.SetAxis("6: PWM 2 Axis 3", PlayerOneIndex, buttons[5].pwm2Neg, buttons[5].pwm2Plus, false);
            axes[5].pwm3Axes = InputControl.SetAxis("6: PWM 3 Axis 5", PlayerOneIndex, buttons[5].pwm3Neg, buttons[5].pwm3Plus, false);
            axes[5].pwm4Axes = InputControl.SetAxis("6: PWM 4 Axis 6", PlayerOneIndex, buttons[5].pwm4Neg, buttons[5].pwm4Plus, false);
            axes[5].pwm5Axes = InputControl.SetAxis("6: PWM 5 Axis 7", PlayerOneIndex, buttons[5].pwm5Neg, buttons[5].pwm5Plus, false);
            axes[5].pwm6Axes = InputControl.SetAxis("6: PWM 6 Axis 8", PlayerOneIndex, buttons[5].pwm6Neg, buttons[5].pwm6Plus, false);
            axes[5].pwm7Axes = InputControl.SetAxis("6: PWM 7 Axis 9", PlayerOneIndex, buttons[5].pwm7Neg, buttons[5].pwm7Plus, false);
            axes[5].pwm8Axes = InputControl.SetAxis("6: PWM 8 Axis 10", PlayerOneIndex, buttons[5].pwm8Neg, buttons[5].pwm8Plus, false);
            axes[5].pwm9Axes = InputControl.SetAxis("6: PWM 9 Axis 11", PlayerOneIndex, buttons[5].pwm9Neg, buttons[5].pwm9Plus, false);
            #endregion
        }

        /// <summary>
        /// Default settings for TankDrive controls.
        /// Adapted from: https://github.com/Gris87/InputControl
        /// </summary>
        public static void TankControls()
        {
            #region Primary Controls
            //Tank controls
            buttons[0].tankFrontLeft = InputControl.SetKey("1: Tank Front Left", PlayerOneIndex, new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick1), true);
            buttons[0].tankBackLeft = InputControl.SetKey("1: Tank Back Left", PlayerOneIndex, new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick1), true);
            buttons[0].tankFrontRight = InputControl.SetKey("1: Tank Front Right", PlayerOneIndex, new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick1), true);
            buttons[0].tankBackRight = InputControl.SetKey("1: Tank Back Right", PlayerOneIndex, new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick1), true);

            //Remaining PWM controls
            buttons[0].pwm2Plus = InputControl.SetKey("1: PWM 2 Positive", PlayerOneIndex, KeyCode.Alpha1, true);
            buttons[0].pwm2Neg = InputControl.SetKey("1: PWM 2 Negative", PlayerOneIndex, KeyCode.Alpha2, true);
            buttons[0].pwm3Plus = InputControl.SetKey("1: PWM 3 Positive", PlayerOneIndex, KeyCode.Alpha3, true);
            buttons[0].pwm3Neg = InputControl.SetKey("1: PWM 3 Negative", PlayerOneIndex, KeyCode.Alpha4, true);
            buttons[0].pwm4Plus = InputControl.SetKey("1: PWM 4 Positive", PlayerOneIndex, KeyCode.Alpha5, true);
            buttons[0].pwm4Neg = InputControl.SetKey("1: PWM 4 Negative", PlayerOneIndex, KeyCode.Alpha6, true);
            buttons[0].pwm5Plus = InputControl.SetKey("1: PWM 5 Positive", PlayerOneIndex, KeyCode.Alpha7, true);
            buttons[0].pwm5Neg = InputControl.SetKey("1: PWM 5 Negative", PlayerOneIndex, KeyCode.Alpha8, true);
            buttons[0].pwm6Plus = InputControl.SetKey("1: PWM 6 Positive", PlayerOneIndex, KeyCode.Alpha9, true);
            buttons[0].pwm6Neg = InputControl.SetKey("1: PWM 6 Negative", PlayerOneIndex, KeyCode.Alpha0, true);
            buttons[0].pwm7Plus = InputControl.SetKey("1: PWM 7 Positive", PlayerOneIndex, KeyCode.Slash, true);
            buttons[0].pwm7Neg = InputControl.SetKey("1: PWM 7 Negative", PlayerOneIndex, KeyCode.Period, true);
            buttons[0].pwm8Plus = InputControl.SetKey("1: PWM 8 Positive", PlayerOneIndex, KeyCode.Comma, true);
            buttons[0].pwm8Neg = InputControl.SetKey("1: PWM 8 Negative", PlayerOneIndex, KeyCode.M, true);
            buttons[0].pwm9Plus = InputControl.SetKey("1: PWM 9 Positive", PlayerOneIndex, KeyCode.N, true);
            buttons[0].pwm9Neg = InputControl.SetKey("1: PWM 9 Negative", PlayerOneIndex, KeyCode.B, true);

            //Other Controls
            buttons[0].resetRobot = InputControl.SetKey("1: Reset Robot", PlayerOneIndex, KeyCode.R, true);
            buttons[0].resetField = InputControl.SetKey("1: Reset Field", PlayerOneIndex, KeyCode.F, true);
            buttons[0].cameraToggle = InputControl.SetKey("1: Camera Toggle", PlayerOneIndex, KeyCode.C, true);
            buttons[0].pickupPrimary = InputControl.SetKey("1: Pick Up Primary Gamepiece", PlayerOneIndex, KeyCode.LeftControl, true);
            buttons[0].releasePrimary = InputControl.SetKey("1: Release Primary Gamepiece", PlayerOneIndex, KeyCode.LeftShift, true);
            buttons[0].spawnPrimary = InputControl.SetKey("1: Spawn Primary Gamepiece", PlayerOneIndex, KeyCode.LeftBracket, true);
            buttons[0].pickupSecondary = InputControl.SetKey("1: Pick Up Secondary Gamepiece", PlayerOneIndex, KeyCode.LeftControl, true);
            buttons[0].releaseSecondary = InputControl.SetKey("1: Release Secondary Gamepiece", PlayerOneIndex, KeyCode.LeftShift, true);
            buttons[0].spawnSecondary = InputControl.SetKey("1: Spawn Secondary Gamepiece", PlayerOneIndex, KeyCode.RightBracket, true);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[0].tankLeftAxes = InputControl.SetAxis("1: Joystick 1 Axis 9", PlayerOneIndex, buttons[0].tankBackLeft, buttons[0].tankFrontLeft, true);
            axes[0].tankRightAxes = InputControl.SetAxis("1: Joystick 1 Axis 10", PlayerOneIndex, buttons[0].tankFrontRight, buttons[0].tankBackRight, true);

            //Set PWM Axes
            axes[0].pwm2Axes = InputControl.SetAxis("1: PWM 2 Axis 3", PlayerOneIndex, buttons[0].pwm2Neg, buttons[0].pwm2Plus, true);
            axes[0].pwm3Axes = InputControl.SetAxis("1: PWM 3 Axis 5", PlayerOneIndex, buttons[0].pwm3Neg, buttons[0].pwm3Plus, true);
            axes[0].pwm4Axes = InputControl.SetAxis("1: PWM 4 Axis 6", PlayerOneIndex, buttons[0].pwm4Neg, buttons[0].pwm4Plus, true);
            axes[0].pwm5Axes = InputControl.SetAxis("1: PWM 5 Axis 7", PlayerOneIndex, buttons[0].pwm5Neg, buttons[0].pwm5Plus, true);
            axes[0].pwm6Axes = InputControl.SetAxis("1: PWM 6 Axis 8", PlayerOneIndex, buttons[0].pwm6Neg, buttons[0].pwm6Plus, true);
            axes[0].pwm7Axes = InputControl.SetAxis("1: PWM 7 Axis 9", PlayerOneIndex, buttons[0].pwm7Neg, buttons[0].pwm7Plus, true);
            axes[0].pwm8Axes = InputControl.SetAxis("1: PWM 8 Axis 10", PlayerOneIndex, buttons[0].pwm8Neg, buttons[0].pwm8Plus, true);
            axes[0].pwm9Axes = InputControl.SetAxis("1: PWM 9 Axis 11", PlayerOneIndex, buttons[0].pwm9Neg, buttons[0].pwm9Plus, true);
            #endregion

            #region Player 2 Controls
            //Tank Controls
            buttons[1].tankFrontLeft = InputControl.SetKey("2: Tank Front Left", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick2), true);
            buttons[1].tankBackLeft = InputControl.SetKey("2: Tank Back Left", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick2), true);
            buttons[1].tankFrontRight = InputControl.SetKey("2: Tank Front Right", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick2), true);
            buttons[1].tankBackRight = InputControl.SetKey("2: Tank Back Right", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick2), true);

            //Remaining PWM controls
            buttons[1].pwm2Plus = InputControl.SetKey("2: PWM 2 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick2), true);
            buttons[1].pwm2Neg = InputControl.SetKey("2: PWM 2 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick2), true);
            buttons[1].pwm3Plus = InputControl.SetKey("2: PWM 3 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick2), true);
            buttons[1].pwm3Neg = InputControl.SetKey("2: PWM 3 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick2), true);
            buttons[1].pwm4Plus = InputControl.SetKey("2: PWM 4 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick2), true);
            buttons[1].pwm4Neg = InputControl.SetKey("2: PWM 4 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick2), true);
            buttons[1].pwm5Plus = InputControl.SetKey("2: PWM 5 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick2), true);
            buttons[1].pwm5Neg = InputControl.SetKey("2: PWM 5 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick2), true);
            buttons[1].pwm6Plus = InputControl.SetKey("2: PWM 6 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick2), true);
            buttons[1].pwm6Neg = InputControl.SetKey("2: PWM 6 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick2), true);
            buttons[1].pwm7Plus = InputControl.SetKey("2: PWM 7 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick2), true);
            buttons[1].pwm7Neg = InputControl.SetKey("2: PWM 7 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick2), true);
            buttons[1].pwm8Plus = InputControl.SetKey("2: PWM 8 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick2), true);
            buttons[1].pwm8Neg = InputControl.SetKey("2: PWM 8 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick2), true);
            buttons[1].pwm9Plus = InputControl.SetKey("2: PWM 9 Positive", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis11Positive, Joystick.Joystick2), true);
            buttons[1].pwm9Neg = InputControl.SetKey("2: PWM 9 Negative", PlayerTwoIndex, new JoystickInput(JoystickAxis.Axis11Negative, Joystick.Joystick2), true);

            //Other Controls
            buttons[1].resetRobot = InputControl.SetKey("2: Reset Robot", PlayerTwoIndex, new JoystickInput(JoystickButton.Button8, Joystick.Joystick2), true);
            buttons[1].resetField = InputControl.SetKey("2: Reset Field", PlayerTwoIndex, new JoystickInput(JoystickButton.Button9, Joystick.Joystick2), true);
            //buttons[1].replayMode = InputControl.setKey("2: Replay Mode", PlayerTwoIndex, KeyCode.Tab, new JoystickInput(JoystickButton.Button6, Joystick.Joystick1), true);
            buttons[1].pickupPrimary = InputControl.SetKey("2: Pick Up Primary Gamepiece", PlayerTwoIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick2), true);
            buttons[1].releasePrimary = InputControl.SetKey("2: Release Primary Gamepiece", PlayerTwoIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick2), true);
            buttons[1].spawnPrimary = InputControl.SetKey("2: Spawn Primary Gamepiece", PlayerTwoIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick2), true);
            buttons[1].pickupSecondary = InputControl.SetKey("2: Pick Up Secondary Gamepiece", PlayerTwoIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick2), true);
            buttons[1].releaseSecondary = InputControl.SetKey("2: Release Secondary Gamepiece", PlayerTwoIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick2), true);
            buttons[1].spawnSecondary = InputControl.SetKey("2: Spawn Secondary Gamepiece", PlayerTwoIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick2), true);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[1].tankLeftAxes = InputControl.SetAxis("Joystick 2 Axis 9", PlayerTwoIndex, buttons[1].tankBackLeft, buttons[1].tankFrontLeft, true);
            axes[1].tankRightAxes = InputControl.SetAxis("Joystick 2 Axis 10", PlayerTwoIndex, buttons[1].tankFrontRight, buttons[1].tankBackRight, true);

            //Set PWM Axes
            axes[1].pwm2Axes = InputControl.SetAxis("2: PWM 2 Axis 3", PlayerOneIndex, buttons[1].pwm2Neg, buttons[1].pwm2Plus, true);
            axes[1].pwm3Axes = InputControl.SetAxis("2: PWM 3 Axis 5", PlayerOneIndex, buttons[1].pwm3Neg, buttons[1].pwm3Plus, true);
            axes[1].pwm4Axes = InputControl.SetAxis("2: PWM 4 Axis 6", PlayerOneIndex, buttons[1].pwm4Neg, buttons[1].pwm4Plus, true);
            axes[1].pwm5Axes = InputControl.SetAxis("2: PWM 5 Axis 7", PlayerOneIndex, buttons[1].pwm5Neg, buttons[1].pwm5Plus, true);
            axes[1].pwm6Axes = InputControl.SetAxis("2: PWM 6 Axis 8", PlayerOneIndex, buttons[1].pwm6Neg, buttons[1].pwm6Plus, true);
            axes[1].pwm7Axes = InputControl.SetAxis("2: PWM 7 Axis 9", PlayerOneIndex, buttons[1].pwm7Neg, buttons[1].pwm7Plus, true);
            axes[1].pwm8Axes = InputControl.SetAxis("2: PWM 8 Axis 10", PlayerOneIndex, buttons[1].pwm8Neg, buttons[1].pwm8Plus, true);
            axes[1].pwm9Axes = InputControl.SetAxis("2: PWM 9 Axis 11", PlayerOneIndex, buttons[1].pwm9Neg, buttons[1].pwm9Plus, true);
            #endregion

            #region Player 3 Controls
            //Tank Controls
            buttons[2].tankFrontLeft = InputControl.SetKey("3: Tank Front Left", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick3), true);
            buttons[2].tankBackLeft = InputControl.SetKey("3: Tank Back Left", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick3), true);
            buttons[2].tankFrontRight = InputControl.SetKey("3: Tank Front Right", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick3), true);
            buttons[2].tankBackRight = InputControl.SetKey("3: Tank Back Right", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick3), true);

            //Remaining PWM controls
            buttons[2].pwm2Plus = InputControl.SetKey("3: PWM 2 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick3), true);
            buttons[2].pwm2Neg = InputControl.SetKey("3: PWM 2 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick3), true);
            buttons[2].pwm3Plus = InputControl.SetKey("3: PWM 3 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick3), true);
            buttons[2].pwm3Neg = InputControl.SetKey("3: PWM 3 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick3), true);
            buttons[2].pwm4Plus = InputControl.SetKey("3: PWM 4 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick3), true);
            buttons[2].pwm4Neg = InputControl.SetKey("3: PWM 4 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick3), true);
            buttons[2].pwm5Plus = InputControl.SetKey("3: PWM 5 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick3), true);
            buttons[2].pwm5Neg = InputControl.SetKey("3: PWM 5 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick3), true);
            buttons[2].pwm6Plus = InputControl.SetKey("3: PWM 6 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick3), true);
            buttons[2].pwm6Neg = InputControl.SetKey("3: PWM 6 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick3), true);
            buttons[2].pwm7Plus = InputControl.SetKey("3: PWM 7 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick3), true);
            buttons[2].pwm7Neg = InputControl.SetKey("3: PWM 7 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick3), true);
            buttons[2].pwm8Plus = InputControl.SetKey("3: PWM 8 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick3), true);
            buttons[2].pwm8Neg = InputControl.SetKey("3: PWM 8 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick3), true);
            buttons[2].pwm9Plus = InputControl.SetKey("3: PWM 9 Positive", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis11Positive, Joystick.Joystick3), true);
            buttons[2].pwm9Neg = InputControl.SetKey("3: PWM 9 Negative", PlayerThreeIndex, new JoystickInput(JoystickAxis.Axis11Negative, Joystick.Joystick3), true);

            //Other Controls
            buttons[2].resetRobot = InputControl.SetKey("3: Reset Robot", PlayerThreeIndex, new JoystickInput(JoystickButton.Button8, Joystick.Joystick3), true);
            buttons[2].resetField = InputControl.SetKey("3: Reset Field", PlayerThreeIndex, new JoystickInput(JoystickButton.Button9, Joystick.Joystick3), true);
            //buttons[2].replayMode = InputControl.setKey("3: Replay Mode", PlayerThreeIndex, KeyCode.Tab, new JoystickInput(JoystickButton.Button6, Joystick.Joystick1), true);
            buttons[2].pickupPrimary = InputControl.SetKey("3: Pick Up Primary Gamepiece", PlayerThreeIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick3), true);
            buttons[2].releasePrimary = InputControl.SetKey("3: Release Primary Gamepiece", PlayerThreeIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick3), true);
            buttons[2].spawnPrimary = InputControl.SetKey("3: Spawn Primary Gamepiece", PlayerThreeIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick3), true);
            buttons[2].pickupSecondary = InputControl.SetKey("3: Pick Up Secondary Gamepiece", PlayerThreeIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick3), true);
            buttons[2].releaseSecondary = InputControl.SetKey("3: Release Secondary Gamepiece", PlayerThreeIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick3), true);
            buttons[2].spawnSecondary = InputControl.SetKey("3: Spawn Secondary Gamepiece", PlayerThreeIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick3), true);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[2].tankLeftAxes = InputControl.SetAxis("Joystick 3 Axis 9", PlayerThreeIndex, buttons[2].tankBackLeft, buttons[2].tankFrontLeft, true);
            axes[2].tankRightAxes = InputControl.SetAxis("Joystick 3 Axis 10", PlayerThreeIndex, buttons[2].tankFrontRight, buttons[2].tankBackRight, true);

            //Set PWM Axes
            axes[2].pwm2Axes = InputControl.SetAxis("3: PWM 2 Axis 3", PlayerOneIndex, buttons[2].pwm2Neg, buttons[2].pwm2Plus, true);
            axes[2].pwm3Axes = InputControl.SetAxis("3: PWM 3 Axis 5", PlayerOneIndex, buttons[2].pwm3Neg, buttons[2].pwm3Plus, true);
            axes[2].pwm4Axes = InputControl.SetAxis("3: PWM 4 Axis 6", PlayerOneIndex, buttons[2].pwm4Neg, buttons[2].pwm4Plus, true);
            axes[2].pwm5Axes = InputControl.SetAxis("3: PWM 5 Axis 7", PlayerOneIndex, buttons[2].pwm5Neg, buttons[2].pwm5Plus, true);
            axes[2].pwm6Axes = InputControl.SetAxis("3: PWM 6 Axis 8", PlayerOneIndex, buttons[2].pwm6Neg, buttons[2].pwm6Plus, true);
            axes[2].pwm7Axes = InputControl.SetAxis("3: PWM 7 Axis 9", PlayerOneIndex, buttons[2].pwm7Neg, buttons[2].pwm7Plus, true);
            axes[2].pwm8Axes = InputControl.SetAxis("3: PWM 8 Axis 10", PlayerOneIndex, buttons[2].pwm8Neg, buttons[2].pwm8Plus, true);
            axes[2].pwm9Axes = InputControl.SetAxis("3: PWM 9 Axis 11", PlayerOneIndex, buttons[2].pwm9Neg, buttons[2].pwm9Plus, true);
            #endregion

            #region Player 4 Controls
            //Tank Controls
            buttons[3].tankFrontLeft = InputControl.SetKey("4: Tank Front Left", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick4), true);
            buttons[3].tankBackLeft = InputControl.SetKey("4: Tank Back Left", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick4), true);
            buttons[3].tankFrontRight = InputControl.SetKey("4: Tank Front Right", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick4), true);
            buttons[3].tankBackRight = InputControl.SetKey("4: Tank Back Right", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick4), true);

            //Remaining PWM controls
            buttons[3].pwm2Plus = InputControl.SetKey("4: PWM 2 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick4), true);
            buttons[3].pwm2Neg = InputControl.SetKey("4: PWM 2 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick4), true);
            buttons[3].pwm3Plus = InputControl.SetKey("4: PWM 3 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick4), true);
            buttons[3].pwm3Neg = InputControl.SetKey("4: PWM 3 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick4), true);
            buttons[3].pwm4Plus = InputControl.SetKey("4: PWM 4 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick4), true);
            buttons[3].pwm4Neg = InputControl.SetKey("4: PWM 4 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick4), true);
            buttons[3].pwm5Plus = InputControl.SetKey("4: PWM 5 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick4), true);
            buttons[3].pwm5Neg = InputControl.SetKey("4: PWM 5 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick4), true);
            buttons[3].pwm6Plus = InputControl.SetKey("4: PWM 6 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick4), true);
            buttons[3].pwm6Neg = InputControl.SetKey("4: PWM 6 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick4), true);
            buttons[3].pwm7Plus = InputControl.SetKey("4: PWM 7 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick4), true);
            buttons[3].pwm7Neg = InputControl.SetKey("4: PWM 7 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick4), true);
            buttons[3].pwm8Plus = InputControl.SetKey("4: PWM 8 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick4), true);
            buttons[3].pwm8Neg = InputControl.SetKey("4: PWM 8 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick4), true);
            buttons[3].pwm9Plus = InputControl.SetKey("4: PWM 9 Positive", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis11Positive, Joystick.Joystick4), true);
            buttons[3].pwm9Neg = InputControl.SetKey("4: PWM 9 Negative", PlayerFourIndex, new JoystickInput(JoystickAxis.Axis11Negative, Joystick.Joystick4), true);

            //Other Controls
            buttons[3].resetRobot = InputControl.SetKey("4: Reset Robot", PlayerFourIndex, new JoystickInput(JoystickButton.Button8, Joystick.Joystick4), true);
            buttons[3].resetField = InputControl.SetKey("4: Reset Field", PlayerFourIndex, new JoystickInput(JoystickButton.Button9, Joystick.Joystick4), true);
            //buttons[3].replayMode = InputControl.setKey("4: Replay Mode", PlayerFourIndex, KeyCode.Tab, new JoystickInput(JoystickButton.Button6, Joystick.Joystick1), true);
            buttons[3].pickupPrimary = InputControl.SetKey("4: Pick Up Primary Gamepiece", PlayerFourIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick4), true);
            buttons[3].releasePrimary = InputControl.SetKey("4: Release Primary Gamepiece", PlayerFourIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick4), true);
            buttons[3].spawnPrimary = InputControl.SetKey("4: Spawn Primary Gamepiece", PlayerFourIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick4), true);
            buttons[3].pickupSecondary = InputControl.SetKey("4: Pick Up Secondary Gamepiece", PlayerFourIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick4), true);
            buttons[3].releaseSecondary = InputControl.SetKey("4: Release Secondary Gamepiece", PlayerFourIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick4), true);
            buttons[3].spawnSecondary = InputControl.SetKey("4: Spawn Secondary Gamepiece", PlayerFourIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick4), true);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[3].tankLeftAxes = InputControl.SetAxis("Joystick 4 Axis 9", PlayerFourIndex, buttons[3].tankBackLeft, buttons[3].tankFrontLeft, true);
            axes[3].tankRightAxes = InputControl.SetAxis("Joystick 4 Axis 10", PlayerFourIndex, buttons[3].tankFrontRight, buttons[3].tankBackRight, true);

            //Set PWM Axes
            axes[3].pwm2Axes = InputControl.SetAxis("4: PWM 2 Axis 3", PlayerOneIndex, buttons[3].pwm2Neg, buttons[3].pwm2Plus, true);
            axes[3].pwm3Axes = InputControl.SetAxis("4: PWM 3 Axis 5", PlayerOneIndex, buttons[3].pwm3Neg, buttons[3].pwm3Plus, true);
            axes[3].pwm4Axes = InputControl.SetAxis("4: PWM 4 Axis 6", PlayerOneIndex, buttons[3].pwm4Neg, buttons[3].pwm4Plus, true);
            axes[3].pwm5Axes = InputControl.SetAxis("4: PWM 5 Axis 7", PlayerOneIndex, buttons[3].pwm5Neg, buttons[3].pwm5Plus, true);
            axes[3].pwm6Axes = InputControl.SetAxis("4: PWM 6 Axis 8", PlayerOneIndex, buttons[3].pwm6Neg, buttons[3].pwm6Plus, true);
            axes[3].pwm7Axes = InputControl.SetAxis("4: PWM 7 Axis 9", PlayerOneIndex, buttons[3].pwm7Neg, buttons[3].pwm7Plus, true);
            axes[3].pwm8Axes = InputControl.SetAxis("4: PWM 8 Axis 10", PlayerOneIndex, buttons[3].pwm8Neg, buttons[3].pwm8Plus, true);
            axes[3].pwm9Axes = InputControl.SetAxis("4: PWM 9 Axis 11", PlayerOneIndex, buttons[3].pwm9Neg, buttons[3].pwm9Plus, true);
            #endregion

            #region Player 5 Controls
            //Tank Controls
            buttons[4].tankFrontLeft = InputControl.SetKey("5: Tank Front Left", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick5), true);
            buttons[4].tankBackLeft = InputControl.SetKey("5: Tank Back Left", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick5), true);
            buttons[4].tankFrontRight = InputControl.SetKey("5: Tank Front Right", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick5), true);
            buttons[4].tankBackRight = InputControl.SetKey("5: Tank Back Right", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick5), true);

            //Remaining PWM controls
            buttons[4].pwm2Plus = InputControl.SetKey("5: PWM 2 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick5), true);
            buttons[4].pwm2Neg = InputControl.SetKey("5: PWM 2 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick5), true);
            buttons[4].pwm3Plus = InputControl.SetKey("5: PWM 3 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick5), true);
            buttons[4].pwm3Neg = InputControl.SetKey("5: PWM 3 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick5), true);
            buttons[4].pwm4Plus = InputControl.SetKey("5: PWM 4 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick5), true);
            buttons[4].pwm4Neg = InputControl.SetKey("5: PWM 4 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick5), true);
            buttons[4].pwm5Plus = InputControl.SetKey("5: PWM 5 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick5), true);
            buttons[4].pwm5Neg = InputControl.SetKey("5: PWM 5 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick5), true);
            buttons[4].pwm6Plus = InputControl.SetKey("5: PWM 6 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick5), true);
            buttons[4].pwm6Neg = InputControl.SetKey("5: PWM 6 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick5), true);
            buttons[4].pwm7Plus = InputControl.SetKey("5: PWM 7 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick5), true);
            buttons[4].pwm7Neg = InputControl.SetKey("5: PWM 7 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick5), true);
            buttons[4].pwm8Plus = InputControl.SetKey("5: PWM 8 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick5), true);
            buttons[4].pwm8Neg = InputControl.SetKey("5: PWM 8 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick5), true);
            buttons[4].pwm9Plus = InputControl.SetKey("5: PWM 9 Positive", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis11Positive, Joystick.Joystick5), true);
            buttons[4].pwm9Neg = InputControl.SetKey("5: PWM 9 Negative", PlayerFiveIndex, new JoystickInput(JoystickAxis.Axis11Negative, Joystick.Joystick5), true);

            //Other Controls
            buttons[4].resetRobot = InputControl.SetKey("5: Reset Robot", PlayerFiveIndex, new JoystickInput(JoystickButton.Button8, Joystick.Joystick5), true);
            buttons[4].resetField = InputControl.SetKey("5: Reset Field", PlayerFiveIndex, new JoystickInput(JoystickButton.Button9, Joystick.Joystick5), true);
            //buttons[4].replayMode = InputControl.setKey("5: Replay Mode", PlayerFiveIndex, KeyCode.Tab, new JoystickInput(JoystickButton.Button6, Joystick.Joystick1), true);
            buttons[4].pickupPrimary = InputControl.SetKey("5: Pick Up Primary Gamepiece", PlayerFiveIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick5), true);
            buttons[4].releasePrimary = InputControl.SetKey("5: Release Primary Gamepiece", PlayerFiveIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick5), true);
            buttons[4].spawnPrimary = InputControl.SetKey("5: Spawn Primary Gamepiece", PlayerFiveIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick5), true);
            buttons[4].pickupSecondary = InputControl.SetKey("5: Pick Up Secondary Gamepiece", PlayerFiveIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick5), true);
            buttons[4].releaseSecondary = InputControl.SetKey("5: Release Secondary Gamepiece", PlayerFiveIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick5), true);
            buttons[4].spawnSecondary = InputControl.SetKey("5: Spawn Secondary Gamepiece", PlayerFiveIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick5), true);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[4].tankLeftAxes = InputControl.SetAxis("Joystick 5 Axis 9", PlayerFiveIndex, buttons[4].tankBackLeft, buttons[4].tankFrontLeft, true);
            axes[4].tankRightAxes = InputControl.SetAxis("Joystick 5 Axis 10", PlayerFiveIndex, buttons[4].tankFrontRight, buttons[4].tankBackRight, true);

            //Set PWM Axes
            axes[4].pwm2Axes = InputControl.SetAxis("5: PWM 2 Axis 3", PlayerOneIndex, buttons[4].pwm2Neg, buttons[4].pwm2Plus, true);
            axes[4].pwm3Axes = InputControl.SetAxis("5: PWM 3 Axis 5", PlayerOneIndex, buttons[4].pwm3Neg, buttons[4].pwm3Plus, true);
            axes[4].pwm4Axes = InputControl.SetAxis("5: PWM 4 Axis 6", PlayerOneIndex, buttons[4].pwm4Neg, buttons[4].pwm4Plus, true);
            axes[4].pwm5Axes = InputControl.SetAxis("5: PWM 5 Axis 7", PlayerOneIndex, buttons[4].pwm5Neg, buttons[4].pwm5Plus, true);
            axes[4].pwm6Axes = InputControl.SetAxis("5: PWM 6 Axis 8", PlayerOneIndex, buttons[4].pwm6Neg, buttons[4].pwm6Plus, true);
            axes[4].pwm7Axes = InputControl.SetAxis("5: PWM 7 Axis 9", PlayerOneIndex, buttons[4].pwm7Neg, buttons[4].pwm7Plus, true);
            axes[4].pwm8Axes = InputControl.SetAxis("5: PWM 8 Axis 10", PlayerOneIndex, buttons[4].pwm8Neg, buttons[4].pwm8Plus, true);
            axes[4].pwm9Axes = InputControl.SetAxis("5: PWM 9 Axis 11", PlayerOneIndex, buttons[4].pwm9Neg, buttons[4].pwm9Plus, true);
            #endregion

            #region Player 6 Controls
            //Tank Controls
            buttons[5].tankFrontLeft = InputControl.SetKey("6: Tank Front Left", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick6), true);
            buttons[5].tankBackLeft = InputControl.SetKey("6: Tank Back Left", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick6), true);
            buttons[5].tankFrontRight = InputControl.SetKey("6: Tank Front Right", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick6), true);
            buttons[5].tankBackRight = InputControl.SetKey("6: Tank Back Right", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick6), true);

            //Remaining PWM controls
            buttons[5].pwm2Plus = InputControl.SetKey("6: PWM 2 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick6), true);
            buttons[5].pwm2Neg = InputControl.SetKey("6: PWM 2 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick6), true);
            buttons[5].pwm3Plus = InputControl.SetKey("6: PWM 3 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick6), true);
            buttons[5].pwm3Neg = InputControl.SetKey("6: PWM 3 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick6), true);
            buttons[5].pwm4Plus = InputControl.SetKey("6: PWM 4 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick6), true);
            buttons[5].pwm4Neg = InputControl.SetKey("6: PWM 4 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick6), true);
            buttons[5].pwm5Plus = InputControl.SetKey("6: PWM 5 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick6), true);
            buttons[5].pwm5Neg = InputControl.SetKey("6: PWM 5 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick6), true);
            buttons[5].pwm6Plus = InputControl.SetKey("6: PWM 6 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick6), true);
            buttons[5].pwm6Neg = InputControl.SetKey("6: PWM 6 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick6), true);
            buttons[5].pwm7Plus = InputControl.SetKey("6: PWM 7 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick6), true);
            buttons[5].pwm7Neg = InputControl.SetKey("6: PWM 7 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick6), true);
            buttons[5].pwm8Plus = InputControl.SetKey("6: PWM 8 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick6), true);
            buttons[5].pwm8Neg = InputControl.SetKey("6: PWM 8 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick6), true);
            buttons[5].pwm9Plus = InputControl.SetKey("6: PWM 9 Positive", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis11Positive, Joystick.Joystick6), true);
            buttons[5].pwm9Neg = InputControl.SetKey("6: PWM 9 Negative", PlayerSixIndex, new JoystickInput(JoystickAxis.Axis11Negative, Joystick.Joystick6), true);

            //Other Controls
            buttons[5].resetRobot = InputControl.SetKey("6: Reset Robot", PlayerSixIndex, new JoystickInput(JoystickButton.Button8, Joystick.Joystick6), true);
            buttons[5].resetField = InputControl.SetKey("6: Reset Field", PlayerSixIndex, new JoystickInput(JoystickButton.Button9, Joystick.Joystick6), true);
            //buttons[5].replayMode = InputControl.setKey("6: Replay Mode", PlayerSixIndex, KeyCode.Tab, new JoystickInput(JoystickButton.Button6, Joystick.Joystick1), true);
            buttons[5].pickupPrimary = InputControl.SetKey("6: Pick Up Primary Gamepiece", PlayerSixIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick6), true);
            buttons[5].releasePrimary = InputControl.SetKey("6: Release Primary Gamepiece", PlayerSixIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick6), true);
            buttons[5].spawnPrimary = InputControl.SetKey("6: Spawn Primary Gamepiece", PlayerSixIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick6), true);
            buttons[5].pickupSecondary = InputControl.SetKey("6: Pick Up Secondary Gamepiece", PlayerSixIndex, new JoystickInput(JoystickButton.Button3, Joystick.Joystick6), true);
            buttons[5].releaseSecondary = InputControl.SetKey("6: Release Secondary Gamepiece", PlayerSixIndex, new JoystickInput(JoystickButton.Button4, Joystick.Joystick6), true);
            buttons[5].spawnSecondary = InputControl.SetKey("6: Spawn Secondary Gamepiece", PlayerSixIndex, new JoystickInput(JoystickButton.Button5, Joystick.Joystick6), true);

            //Set Arcade Drive Axes (PWM [0] and PWM [1])
            axes[5].tankLeftAxes = InputControl.SetAxis("Joystick 6 Axis 9", PlayerSixIndex, buttons[5].tankBackLeft, buttons[5].tankFrontLeft, true);
            axes[5].tankRightAxes = InputControl.SetAxis("Joystick 6 Axis 10", PlayerSixIndex, buttons[5].tankFrontRight, buttons[5].tankBackRight, true);

            //Set PWM Axes
            axes[5].pwm2Axes = InputControl.SetAxis("6: PWM 2 Axis 3", PlayerOneIndex, buttons[5].pwm2Neg, buttons[5].pwm2Plus, true);
            axes[5].pwm3Axes = InputControl.SetAxis("6: PWM 3 Axis 5", PlayerOneIndex, buttons[5].pwm3Neg, buttons[5].pwm3Plus, true);
            axes[5].pwm4Axes = InputControl.SetAxis("6: PWM 4 Axis 6", PlayerOneIndex, buttons[5].pwm4Neg, buttons[5].pwm4Plus, true);
            axes[5].pwm5Axes = InputControl.SetAxis("6: PWM 5 Axis 7", PlayerOneIndex, buttons[5].pwm5Neg, buttons[5].pwm5Plus, true);
            axes[5].pwm6Axes = InputControl.SetAxis("6: PWM 6 Axis 8", PlayerOneIndex, buttons[5].pwm6Neg, buttons[5].pwm6Plus, true);
            axes[5].pwm7Axes = InputControl.SetAxis("6: PWM 7 Axis 9", PlayerOneIndex, buttons[5].pwm7Neg, buttons[5].pwm7Plus, true);
            axes[5].pwm8Axes = InputControl.SetAxis("6: PWM 8 Axis 10", PlayerOneIndex, buttons[5].pwm8Neg, buttons[5].pwm8Plus, true);
            axes[5].pwm9Axes = InputControl.SetAxis("6: PWM 9 Axis 11", PlayerOneIndex, buttons[5].pwm9Neg, buttons[5].pwm9Plus, true);
            #endregion
        }
    }


    #region Old Controls: 2016 and Older
    //public static void ResetDefaults()
    //{
    //    ControlKey[(int)Control.Forward] = KeyCode.UpArrow;
    //    ControlKey[(int)Control.Backward] = KeyCode.DownArrow;
    //    ControlKey[(int)Control.Right] = KeyCode.RightArrow;
    //    ControlKey[(int)Control.Left] = KeyCode.LeftArrow;
    //    ControlKey[(int)Control.ResetRobot] = KeyCode.R;
    //    ControlKey[(int)Control.CameraToggle] = KeyCode.C;
    //    ControlKey[(int)Control.pwm2Plus] = KeyCode.Alpha1;
    //    ControlKey[(int)Control.pwm2Neg] = KeyCode.Alpha2;
    //    ControlKey[(int)Control.pwm3Plus] = KeyCode.Alpha3;
    //    ControlKey[(int)Control.pwm3Neg] = KeyCode.Alpha4;
    //    ControlKey[(int)Control.pwm4Plus] = KeyCode.Alpha5;
    //    ControlKey[(int)Control.pwm4Neg] = KeyCode.Alpha6;
    //    ControlKey[(int)Control.pwm5Plus] = KeyCode.Alpha7;
    //    ControlKey[(int)Control.pwm5Neg] = KeyCode.Alpha8;
    //    ControlKey[(int)Control.pwm6Plus] = KeyCode.Alpha9;
    //    ControlKey[(int)Control.pwm6Neg] = KeyCode.Alpha0;
    //    ControlKey[(int)Control.PickupPrimary] = KeyCode.X;
    //    ControlKey[(int)Control.ReleasePrimary] = KeyCode.E;
    //    ControlKey[(int)Control.SpawnPrimary] = KeyCode.Q;
    //    ControlKey[(int)Control.PickupSecondary] = KeyCode.X;
    //    ControlKey[(int)Control.ReleaseSecondary] = KeyCode.E;
    //    ControlKey[(int)Control.SpawnSecondary] = KeyCode.Q;
    //}

    //public static void LoadControls()
    //{
    //    for (int i = 0; i < ControlKey.Length; i++)
    //    {
    //        if (PlayerPrefs.HasKey("ControlKey" + i.ToString())) ControlKey[i] = (KeyCode)PlayerPrefs.GetInt("ControlKey" + i.ToString());
    //    }
    //}

    //public static void SaveControls()
    //{
    //    for (int i = 0; i < ControlKey.Length; i++)
    //    {
    //        PlayerPrefs.SetInt("ControlKey" + i.ToString(), (int)ControlKey[i]);
    //    }
    //    PlayerPrefs.Save();
    //}

    //public static bool SetControl(int control, KeyCode key)
    //{
    //    ControlKey[control] = key;
    //    for (int i = 0; i < ControlKey.Length; i++)
    //    {
    //        if (i != control && ControlKey[i] == key)
    //        {
    //            return false;
    //        }
    //    }
    //    return true;
    //}

    //public static bool CheckConflict()
    //{
    //    for (int i = 0; i < ControlKey.Length; i++)
    //    {
    //        for (int j = 1; j < ControlKey.Length; j++)
    //        {
    //            if (j != i && ControlKey[i] == ControlKey[j]) return true;
    //        }
    //    }
    //    return false;
    //}
    #endregion
}