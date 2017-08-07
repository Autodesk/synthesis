using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

    public static bool IsTankDrive;

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

        //Other controls
        public KeyMapping resetRobot;
        public KeyMapping cameraToggle;
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
        public Axis vertical;
        public Axis horizontal;
    }

    /// <summary>
    /// Set of buttons.
    /// </summary>
    public static Buttons[] buttons = new Buttons[7];

    /// <summary>
    /// Set of axes.
    /// </summary>
    public static Axes[] axes = new Axes[7];

    /// <summary>
    /// Initializes the <see cref="Controls"/> class.
    /// </summary>
    static Controls()
    {
        SwitchControls();
    }

    public static void SwitchControls()
    {
        if (IsTankDrive)
        {
            TankDrive();
            Load();

            Debug.Log("tankDrive true");
        }
        else
        {
            ArcadeDrive();
            Load();
          

            Debug.Log("tankDrive false");
        }
    }

    /// <summary>
    /// Nothing. It just call static constructor if needed.
    /// </summary>
    public static void Init()
    {
        // Nothing. It just call static constructor if needed
    }

    /// <summary>
    /// Save controls.
    /// </summary>
    public static void Save()
    {
        ReadOnlyCollection<KeyMapping> keys = InputControl.getKeysList();

        foreach (KeyMapping key in keys)
        {
            PlayerPrefs.SetString("Controls." + key.name + ".primary", key.primaryInput.ToString());
            PlayerPrefs.SetString("Controls." + key.name + ".secondary", key.secondaryInput.ToString());
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load controls.
    /// </summary>
    public static void Load()
    {
        ReadOnlyCollection<KeyMapping> keys = InputControl.getKeysList();

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
    /// Resets to default controls.
    /// </summary>
    public static void ArcadeDrive()
    {
        //RemoveTankKeys();

        #region Primary Controls
        //Basic robot controls
        buttons[0].forward = InputControl.setKey("Forward", KeyCode.UpArrow, new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick1));
        buttons[0].backward = InputControl.setKey("Backward", KeyCode.DownArrow, new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick1));
        buttons[0].left = InputControl.setKey("Left", KeyCode.LeftArrow, new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick1));
        buttons[0].right = InputControl.setKey("Right", KeyCode.RightArrow, new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick1));

        //Remaining PWM controls
        buttons[0].pwm2Plus = InputControl.setKey("PWM 2 Positive", KeyCode.Alpha1, new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick1));
        buttons[0].pwm2Neg = InputControl.setKey("PWM 2 Negative", KeyCode.Alpha2, new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick1));
        buttons[0].pwm3Plus = InputControl.setKey("PWM 3 Positive", KeyCode.Alpha3, new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick1));
        buttons[0].pwm3Neg = InputControl.setKey("PWM 3 Negative", KeyCode.Alpha4, new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick1));
        buttons[0].pwm4Plus = InputControl.setKey("PWM 4 Positive", KeyCode.Alpha5, new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick1));
        buttons[0].pwm4Neg = InputControl.setKey("PWM 4 Negative", KeyCode.Alpha6, new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick1));
        buttons[0].pwm5Plus = InputControl.setKey("PWM 5 Positive", KeyCode.Alpha7, new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick1));
        buttons[0].pwm5Neg = InputControl.setKey("PWM 5 Negative", KeyCode.Alpha8, new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick1));
        buttons[0].pwm6Plus = InputControl.setKey("PWM 6 Positive", KeyCode.Alpha9, new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick1));
        buttons[0].pwm6Neg = InputControl.setKey("PWM 6 Negative", KeyCode.Alpha0, new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick1));

        //Other Controls
        buttons[0].resetRobot = InputControl.setKey("Reset Robot", KeyCode.R, new JoystickInput(JoystickButton.Button8, Joystick.Joystick1));
        buttons[0].cameraToggle = InputControl.setKey("Camera Toggle", KeyCode.C, new JoystickInput(JoystickButton.Button7, Joystick.Joystick1));
        buttons[0].pickupPrimary = InputControl.setKey("Pick Up Primary Gamepiece", KeyCode.X, new JoystickInput(JoystickButton.Button3, Joystick.Joystick1));
        buttons[0].releasePrimary = InputControl.setKey("Release Primary Gamepiece", KeyCode.E, new JoystickInput(JoystickButton.Button4, Joystick.Joystick1));
        buttons[0].spawnPrimary = InputControl.setKey("Spawn Primary Gamepiece", KeyCode.Q, new JoystickInput(JoystickButton.Button5, Joystick.Joystick1));
        buttons[0].pickupSecondary = InputControl.setKey("Pick Up Secondary Gamepiece", KeyCode.X, new JoystickInput(JoystickButton.Button3, Joystick.Joystick1));
        buttons[0].releaseSecondary = InputControl.setKey("Release Secondary Gamepiece", KeyCode.E, new JoystickInput(JoystickButton.Button4, Joystick.Joystick1));
        buttons[0].spawnSecondary = InputControl.setKey("Spawn Secondary Gamepiece", KeyCode.Q, new JoystickInput(JoystickButton.Button5, Joystick.Joystick1));

        //Set axes
        axes[0].horizontal = InputControl.setAxis("Joystick 1 Axis 2", buttons[0].left, buttons[0].right);
        axes[0].vertical = InputControl.setAxis("Joystick 1 Axis 4", buttons[0].backward, buttons[0].forward);
        #endregion

        #region Player 2 Controls
        //Basic robot controls
        buttons[1].forward = InputControl.setKey("2: Forward", new JoystickInput(JoystickAxis.Axis2Negative,Joystick.Joystick2));
        buttons[1].backward = InputControl.setKey("2: Backward", new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick2));
        buttons[1].left = InputControl.setKey("2: Left", new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick2));
        buttons[1].right = InputControl.setKey("2: Right", new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick2));

        //Remaining PWM controls
        buttons[1].pwm2Plus = InputControl.setKey("2: PWM 2 Positive", new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick2));
        buttons[1].pwm2Neg = InputControl.setKey("2: PWM 2 Negative", new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick2));
        buttons[1].pwm3Plus = InputControl.setKey("2: PWM 3 Positive", new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick2));
        buttons[1].pwm3Neg = InputControl.setKey("2: PWM 3 Negative", new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick2));
        buttons[1].pwm4Plus = InputControl.setKey("2: PWM 4 Positive", new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick2));
        buttons[1].pwm4Neg = InputControl.setKey("2: PWM 4 Negative", new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick2));
        buttons[1].pwm5Plus = InputControl.setKey("2: PWM 5 Positive", new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick2));
        buttons[1].pwm5Neg = InputControl.setKey("2: PWM 5 Negative", new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick2));
        buttons[1].pwm6Plus = InputControl.setKey("2: PWM 6 Positive", new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick2));
        buttons[1].pwm6Neg = InputControl.setKey("2: PWM 6 Negative", new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick2));

        //Other Controls
        buttons[1].resetRobot = InputControl.setKey("2: Reset Robot", new JoystickInput(JoystickButton.Button8, Joystick.Joystick2));
        buttons[1].cameraToggle = InputControl.setKey("2: Camera Toggle", new JoystickInput(JoystickButton.Button7, Joystick.Joystick2));
        buttons[1].pickupPrimary = InputControl.setKey("2: Pick Up Primary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick2));
        buttons[1].releasePrimary = InputControl.setKey("2: Release Primary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick2));
        buttons[1].spawnPrimary = InputControl.setKey("2: Spawn Primary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick2));
        buttons[1].pickupSecondary = InputControl.setKey("2: Pick Up Secondary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick2));
        buttons[1].releaseSecondary = InputControl.setKey("2: Release Secondary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick2));
        buttons[1].spawnSecondary = InputControl.setKey("2: Spawn Secondary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick2));
        #endregion

        #region Player 3 Controls
        //Basic robot controls
        buttons[2].forward = InputControl.setKey("3: Forward", new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick3));
        buttons[2].backward = InputControl.setKey("3: Backward", new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick3));
        buttons[2].left = InputControl.setKey("3: Left", new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick3));
        buttons[2].right = InputControl.setKey("3: Right", new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick3));

        //Remaining PWM controls
        buttons[2].pwm2Plus = InputControl.setKey("3: PWM 2 Positive", new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick3));
        buttons[2].pwm2Neg = InputControl.setKey("3: PWM 2 Negative", new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick3));
        buttons[2].pwm3Plus = InputControl.setKey("3: PWM 3 Positive", new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick3));
        buttons[2].pwm3Neg = InputControl.setKey("3: PWM 3 Negative", new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick3));
        buttons[2].pwm4Plus = InputControl.setKey("3: PWM 4 Positive", new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick3));
        buttons[2].pwm4Neg = InputControl.setKey("3: PWM 4 Negative", new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick3));
        buttons[2].pwm5Plus = InputControl.setKey("3: PWM 5 Positive", new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick3));
        buttons[2].pwm5Neg = InputControl.setKey("3: PWM 5 Negative", new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick3));
        buttons[2].pwm6Plus = InputControl.setKey("3: PWM 6 Positive", new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick3));
        buttons[2].pwm6Neg = InputControl.setKey("3: PWM 6 Negative", new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick3));

        //Other Controls
        buttons[2].resetRobot = InputControl.setKey("3: Reset Robot", new JoystickInput(JoystickButton.Button8, Joystick.Joystick3));
        buttons[2].cameraToggle = InputControl.setKey("3: Camera Toggle", new JoystickInput(JoystickButton.Button7, Joystick.Joystick3));
        buttons[2].pickupPrimary = InputControl.setKey("3: Pick Up Primary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick3));
        buttons[2].releasePrimary = InputControl.setKey("3: Release Primary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick3));
        buttons[2].spawnPrimary = InputControl.setKey("3: Spawn Primary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick3));
        buttons[2].pickupSecondary = InputControl.setKey("3: Pick Up Secondary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick3));
        buttons[2].releaseSecondary = InputControl.setKey("3: Release Secondary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick3));
        buttons[2].spawnSecondary = InputControl.setKey("3: Spawn Secondary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick3));
        #endregion

        #region Player 4 Controls
        //Basic robot controls
        buttons[3].forward = InputControl.setKey("4: Forward", new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick4));
        buttons[3].backward = InputControl.setKey("4: Backward", new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick4));
        buttons[3].left = InputControl.setKey("4: Left", new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick4));
        buttons[3].right = InputControl.setKey("4: Right", new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick4));

        //Remaining PWM controls
        buttons[3].pwm2Plus = InputControl.setKey("4: PWM 2 Positive", new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick4));
        buttons[3].pwm2Neg = InputControl.setKey("4: PWM 2 Negative", new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick4));
        buttons[3].pwm3Plus = InputControl.setKey("4: PWM 3 Positive", new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick4));
        buttons[3].pwm3Neg = InputControl.setKey("4: PWM 3 Negative", new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick4));
        buttons[3].pwm4Plus = InputControl.setKey("4: PWM 4 Positive", new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick4));
        buttons[3].pwm4Neg = InputControl.setKey("4: PWM 4 Negative", new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick4));
        buttons[3].pwm5Plus = InputControl.setKey("4: PWM 5 Positive", new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick4));
        buttons[3].pwm5Neg = InputControl.setKey("4: PWM 5 Negative", new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick4));
        buttons[3].pwm6Plus = InputControl.setKey("4: PWM 6 Positive", new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick4));
        buttons[3].pwm6Neg = InputControl.setKey("4: PWM 6 Negative", new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick4));

        //Other Controls
        buttons[3].resetRobot = InputControl.setKey("4: Reset Robot", new JoystickInput(JoystickButton.Button8, Joystick.Joystick4));
        buttons[3].cameraToggle = InputControl.setKey("4: Camera Toggle", new JoystickInput(JoystickButton.Button7, Joystick.Joystick4));
        buttons[3].pickupPrimary = InputControl.setKey("4: Pick Up Primary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick4));
        buttons[3].releasePrimary = InputControl.setKey("4: Release Primary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick4));
        buttons[3].spawnPrimary = InputControl.setKey("4: Spawn Primary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick4));
        buttons[3].pickupSecondary = InputControl.setKey("4: Pick Up Secondary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick4));
        buttons[3].releaseSecondary = InputControl.setKey("4: Release Secondary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick4));
        buttons[3].spawnSecondary = InputControl.setKey("4: Spawn Secondary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick4));
        #endregion

        #region Player 5 Controls
        //Basic robot controls
        buttons[4].forward = InputControl.setKey("5: Forward", new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick5));
        buttons[4].backward = InputControl.setKey("5: Backward", new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick5));
        buttons[4].left = InputControl.setKey("5: Left", new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick5));
        buttons[4].right = InputControl.setKey("5: Right", new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick5));

        //Remaining PWM controls
        buttons[4].pwm2Plus = InputControl.setKey("5: PWM 2 Positive", new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick5));
        buttons[4].pwm2Neg = InputControl.setKey("5: PWM 2 Negative", new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick5));
        buttons[4].pwm3Plus = InputControl.setKey("5: PWM 3 Positive", new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick5));
        buttons[4].pwm3Neg = InputControl.setKey("5: PWM 3 Negative", new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick5));
        buttons[4].pwm4Plus = InputControl.setKey("5: PWM 4 Positive", new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick5));
        buttons[4].pwm4Neg = InputControl.setKey("5: PWM 4 Negative", new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick5));
        buttons[4].pwm5Plus = InputControl.setKey("5: PWM 5 Positive", new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick5));
        buttons[4].pwm5Neg = InputControl.setKey("5: PWM 5 Negative", new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick5));
        buttons[4].pwm6Plus = InputControl.setKey("5: PWM 6 Positive", new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick5));
        buttons[4].pwm6Neg = InputControl.setKey("5: PWM 6 Negative", new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick5));

        //Other Controls
        buttons[4].resetRobot = InputControl.setKey("5: Reset Robot", new JoystickInput(JoystickButton.Button8, Joystick.Joystick5));
        buttons[4].cameraToggle = InputControl.setKey("5: Camera Toggle", new JoystickInput(JoystickButton.Button7, Joystick.Joystick5));
        buttons[4].pickupPrimary = InputControl.setKey("5: Pick Up Primary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick5));
        buttons[4].releasePrimary = InputControl.setKey("5: Release Primary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick5));
        buttons[4].spawnPrimary = InputControl.setKey("5: Spawn Primary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick5));
        buttons[4].pickupSecondary = InputControl.setKey("5: Pick Up Secondary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick5));
        buttons[4].releaseSecondary = InputControl.setKey("5: Release Secondary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick5));
        buttons[4].spawnSecondary = InputControl.setKey("5: Spawn Secondary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick5));
        #endregion

        #region Player 6 Controls
        //Basic robot controls
        buttons[5].forward = InputControl.setKey("6: Forward", new JoystickInput(JoystickAxis.Axis2Negative, Joystick.Joystick6));
        buttons[5].backward = InputControl.setKey("6: Backward", new JoystickInput(JoystickAxis.Axis2Positive, Joystick.Joystick6));
        buttons[5].left = InputControl.setKey("6: Left", new JoystickInput(JoystickAxis.Axis4Negative, Joystick.Joystick6));
        buttons[5].right = InputControl.setKey("6: Right", new JoystickInput(JoystickAxis.Axis4Positive, Joystick.Joystick6));

        //Remaining PWM controls
        buttons[5].pwm2Plus = InputControl.setKey("6: PWM 2 Positive", new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick6));
        buttons[5].pwm2Neg = InputControl.setKey("6: PWM 2 Negative", new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick6));
        buttons[5].pwm3Plus = InputControl.setKey("6: PWM 3 Positive", new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick6));
        buttons[5].pwm3Neg = InputControl.setKey("6: PWM 3 Negative", new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick6));
        buttons[5].pwm4Plus = InputControl.setKey("6: PWM 4 Positive", new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick6));
        buttons[5].pwm4Neg = InputControl.setKey("6: PWM 4 Negative", new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick6));
        buttons[5].pwm5Plus = InputControl.setKey("6: PWM 5 Positive", new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick6));
        buttons[5].pwm5Neg = InputControl.setKey("6: PWM 5 Negative", new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick6));
        buttons[5].pwm6Plus = InputControl.setKey("6: PWM 6 Positive", new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick6));
        buttons[5].pwm6Neg = InputControl.setKey("6: PWM 6 Negative", new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick6));

        //Other Controls
        buttons[5].resetRobot = InputControl.setKey("6: Reset Robot", new JoystickInput(JoystickButton.Button8, Joystick.Joystick6));
        buttons[5].cameraToggle = InputControl.setKey("6: Camera Toggle", new JoystickInput(JoystickButton.Button7, Joystick.Joystick6));
        buttons[5].pickupPrimary = InputControl.setKey("6: Pick Up Primary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick6));
        buttons[5].releasePrimary = InputControl.setKey("6: Release Primary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick6));
        buttons[5].spawnPrimary = InputControl.setKey("6: Spawn Primary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick6));
        buttons[5].pickupSecondary = InputControl.setKey("6: Pick Up Secondary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick6));
        buttons[5].releaseSecondary = InputControl.setKey("6: Release Secondary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick6));
        buttons[5].spawnSecondary = InputControl.setKey("6: Spawn Secondary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick6));
        #endregion

        IsTankDrive = false;
        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
        //GameObject.Find("Content").GetComponent<CreateButton>().UpdateButtons();
        //if (GameObject.Find("TankMode") != null) GameObject.Find("TankMode").GetComponent<TankMode>().UpdateAllText();
    }

    public static void RemoveArcadeKeys()
    {

        #region Remove Primary Controls
        //Basic robot controls
        InputControl.removeKey(buttons[0].forward);
        InputControl.removeKey(buttons[0].backward);
        InputControl.removeKey(buttons[0].left);
        InputControl.removeKey(buttons[0].right);

        //Remaining PWM controls
        InputControl.removeKey(buttons[0].pwm2Plus);
        InputControl.removeKey(buttons[0].pwm2Neg);
        InputControl.removeKey(buttons[0].pwm3Plus);
        InputControl.removeKey(buttons[0].pwm3Neg);
        InputControl.removeKey(buttons[0].pwm4Plus);
        InputControl.removeKey(buttons[0].pwm4Neg);
        InputControl.removeKey(buttons[0].pwm5Plus);
        InputControl.removeKey(buttons[0].pwm5Neg);
        InputControl.removeKey(buttons[0].pwm6Plus);
        InputControl.removeKey(buttons[0].pwm6Neg);

        //Other controls
        InputControl.removeKey(buttons[0].resetRobot);
        InputControl.removeKey(buttons[0].cameraToggle);
        InputControl.removeKey(buttons[0].pickupPrimary);
        InputControl.removeKey(buttons[0].releasePrimary);
        InputControl.removeKey(buttons[0].spawnPrimary);
        InputControl.removeKey(buttons[0].pickupSecondary);
        InputControl.removeKey(buttons[0].releaseSecondary);
        InputControl.removeKey(buttons[0].spawnSecondary);

        //Set axes
        InputControl.removeAxis(axes[0].horizontal);
        InputControl.removeAxis(axes[0].vertical);
        #endregion

        #region Player 2 Controls
        //Basic robot controls
        InputControl.removeKey(buttons[1].forward);
        InputControl.removeKey(buttons[1].backward);
        InputControl.removeKey(buttons[1].left);
        InputControl.removeKey(buttons[1].right);

        //Remaining PWM controls
        InputControl.removeKey(buttons[1].pwm2Plus);
        InputControl.removeKey(buttons[1].pwm2Neg);
        InputControl.removeKey(buttons[1].pwm3Plus);
        InputControl.removeKey(buttons[1].pwm3Neg);
        InputControl.removeKey(buttons[1].pwm4Plus);
        InputControl.removeKey(buttons[1].pwm4Neg);
        InputControl.removeKey(buttons[1].pwm5Plus);
        InputControl.removeKey(buttons[1].pwm5Neg);
        InputControl.removeKey(buttons[1].pwm6Plus);
        InputControl.removeKey(buttons[1].pwm6Neg);

        //Other controls
        InputControl.removeKey(buttons[1].resetRobot);
        InputControl.removeKey(buttons[1].cameraToggle);
        InputControl.removeKey(buttons[1].pickupPrimary);
        InputControl.removeKey(buttons[1].releasePrimary);
        InputControl.removeKey(buttons[1].spawnPrimary);
        InputControl.removeKey(buttons[1].pickupSecondary);
        InputControl.removeKey(buttons[1].releaseSecondary);
        InputControl.removeKey(buttons[1].spawnSecondary);
        #endregion

        #region Player 3 Controls
        //Basic robot controls
        InputControl.removeKey(buttons[2].forward);
        InputControl.removeKey(buttons[2].backward);
        InputControl.removeKey(buttons[2].left);
        InputControl.removeKey(buttons[2].right);

        //Remaining PWM controls
        InputControl.removeKey(buttons[2].pwm2Plus);
        InputControl.removeKey(buttons[2].pwm2Neg);
        InputControl.removeKey(buttons[2].pwm3Plus);
        InputControl.removeKey(buttons[2].pwm3Neg);
        InputControl.removeKey(buttons[2].pwm4Plus);
        InputControl.removeKey(buttons[2].pwm4Neg);
        InputControl.removeKey(buttons[2].pwm5Plus);
        InputControl.removeKey(buttons[2].pwm5Neg);
        InputControl.removeKey(buttons[2].pwm6Plus);
        InputControl.removeKey(buttons[2].pwm6Neg);

        //Other controls
        InputControl.removeKey(buttons[2].resetRobot);
        InputControl.removeKey(buttons[2].cameraToggle);
        InputControl.removeKey(buttons[2].pickupPrimary);
        InputControl.removeKey(buttons[2].releasePrimary);
        InputControl.removeKey(buttons[2].spawnPrimary);
        InputControl.removeKey(buttons[2].pickupSecondary);
        InputControl.removeKey(buttons[2].releaseSecondary);
        InputControl.removeKey(buttons[2].spawnSecondary);
        #endregion

        #region Player4 Controls
        //Basic robot controls
        InputControl.removeKey(buttons[3].forward);
        InputControl.removeKey(buttons[3].backward);
        InputControl.removeKey(buttons[3].left);
        InputControl.removeKey(buttons[3].right);

        //Remaining PWM controls
        InputControl.removeKey(buttons[3].pwm2Plus);
        InputControl.removeKey(buttons[3].pwm2Neg);
        InputControl.removeKey(buttons[3].pwm3Plus);
        InputControl.removeKey(buttons[3].pwm3Neg);
        InputControl.removeKey(buttons[3].pwm4Plus);
        InputControl.removeKey(buttons[3].pwm4Neg);
        InputControl.removeKey(buttons[3].pwm5Plus);
        InputControl.removeKey(buttons[3].pwm5Neg);
        InputControl.removeKey(buttons[3].pwm6Plus);
        InputControl.removeKey(buttons[3].pwm6Neg);

        //Other controls
        InputControl.removeKey(buttons[3].resetRobot);
        InputControl.removeKey(buttons[3].cameraToggle);
        InputControl.removeKey(buttons[3].pickupPrimary);
        InputControl.removeKey(buttons[3].releasePrimary);
        InputControl.removeKey(buttons[3].spawnPrimary);
        InputControl.removeKey(buttons[3].pickupSecondary);
        InputControl.removeKey(buttons[3].releaseSecondary);
        InputControl.removeKey(buttons[3].spawnSecondary);
        #endregion

        #region Player 5 Controls
        //Basic robot controls
        InputControl.removeKey(buttons[4].forward);
        InputControl.removeKey(buttons[4].backward);
        InputControl.removeKey(buttons[4].left);
        InputControl.removeKey(buttons[4].right);

        //Remaining PWM controls
        InputControl.removeKey(buttons[4].pwm2Plus);
        InputControl.removeKey(buttons[4].pwm2Neg);
        InputControl.removeKey(buttons[4].pwm3Plus);
        InputControl.removeKey(buttons[4].pwm3Neg);
        InputControl.removeKey(buttons[4].pwm4Plus);
        InputControl.removeKey(buttons[4].pwm4Neg);
        InputControl.removeKey(buttons[4].pwm5Plus);
        InputControl.removeKey(buttons[4].pwm5Neg);
        InputControl.removeKey(buttons[4].pwm6Plus);
        InputControl.removeKey(buttons[4].pwm6Neg);

        //Other controls
        InputControl.removeKey(buttons[4].resetRobot);
        InputControl.removeKey(buttons[4].cameraToggle);
        InputControl.removeKey(buttons[4].pickupPrimary);
        InputControl.removeKey(buttons[4].releasePrimary);
        InputControl.removeKey(buttons[4].spawnPrimary);
        InputControl.removeKey(buttons[4].pickupSecondary);
        InputControl.removeKey(buttons[4].releaseSecondary);
        InputControl.removeKey(buttons[4].spawnSecondary);
        #endregion

        #region Player 6 Controls
        //Basic robot controls
        InputControl.removeKey(buttons[5].forward);
        InputControl.removeKey(buttons[5].backward);
        InputControl.removeKey(buttons[5].left);
        InputControl.removeKey(buttons[5].right);

        //Remaining PWM controls
        InputControl.removeKey(buttons[5].pwm2Plus);
        InputControl.removeKey(buttons[5].pwm2Neg);
        InputControl.removeKey(buttons[5].pwm3Plus);
        InputControl.removeKey(buttons[5].pwm3Neg);
        InputControl.removeKey(buttons[5].pwm4Plus);
        InputControl.removeKey(buttons[5].pwm4Neg);
        InputControl.removeKey(buttons[5].pwm5Plus);
        InputControl.removeKey(buttons[5].pwm5Neg);
        InputControl.removeKey(buttons[5].pwm6Plus);
        InputControl.removeKey(buttons[5].pwm6Neg);

        //Other controls
        InputControl.removeKey(buttons[5].resetRobot);
        InputControl.removeKey(buttons[5].cameraToggle);
        InputControl.removeKey(buttons[5].pickupPrimary);
        InputControl.removeKey(buttons[5].releasePrimary);
        InputControl.removeKey(buttons[5].spawnPrimary);
        InputControl.removeKey(buttons[5].pickupSecondary);
        InputControl.removeKey(buttons[5].releaseSecondary);
        InputControl.removeKey(buttons[5].spawnSecondary);
        #endregion
    }

    public static void TankDrive()
    {
        RemoveArcadeKeys();

        #region Primary Controls
        //Tank controls
        buttons[0].tankFrontLeft = InputControl.setKey("Tank Front Left", new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick1));
        buttons[0].tankBackLeft = InputControl.setKey("Tank Back Left", new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick1));
        buttons[0].tankFrontRight = InputControl.setKey("Tank Front Right", new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick1));
        buttons[0].tankBackRight = InputControl.setKey("Tank Back Right", new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick1));

        //Remaining PWM controls
        buttons[0].pwm2Plus = InputControl.setKey("PWM 2 Positive", KeyCode.Alpha1, new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick1));
        buttons[0].pwm2Neg = InputControl.setKey("PWM 2 Negative", KeyCode.Alpha2, new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick1));
        buttons[0].pwm3Plus = InputControl.setKey("PWM 3 Positive", KeyCode.Alpha3, new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick1));
        buttons[0].pwm3Neg = InputControl.setKey("PWM 3 Negative", KeyCode.Alpha4, new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick1));
        buttons[0].pwm4Plus = InputControl.setKey("PWM 4 Positive", KeyCode.Alpha5, new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick1));
        buttons[0].pwm4Neg = InputControl.setKey("PWM 4 Negative", KeyCode.Alpha6, new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick1));
        buttons[0].pwm5Plus = InputControl.setKey("PWM 5 Positive", KeyCode.Alpha7, new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick1));
        buttons[0].pwm5Neg = InputControl.setKey("PWM 5 Negative", KeyCode.Alpha8, new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick1));
        buttons[0].pwm6Plus = InputControl.setKey("PWM 6 Positive", KeyCode.Alpha9, new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick1));
        buttons[0].pwm6Neg = InputControl.setKey("PWM 6 Negative", KeyCode.Alpha0, new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick1));

        //Other Controls
        buttons[0].resetRobot = InputControl.setKey("Reset Robot", KeyCode.R, new JoystickInput(JoystickButton.Button8, Joystick.Joystick1));
        buttons[0].cameraToggle = InputControl.setKey("Camera Toggle", KeyCode.C, new JoystickInput(JoystickButton.Button7, Joystick.Joystick1));
        buttons[0].pickupPrimary = InputControl.setKey("Pick Up Primary Gamepiece", KeyCode.X, new JoystickInput(JoystickButton.Button3, Joystick.Joystick1));
        buttons[0].releasePrimary = InputControl.setKey("Release Primary Gamepiece", KeyCode.E, new JoystickInput(JoystickButton.Button4, Joystick.Joystick1));
        buttons[0].spawnPrimary = InputControl.setKey("Spawn Primary Gamepiece", KeyCode.Q, new JoystickInput(JoystickButton.Button5, Joystick.Joystick1));
        buttons[0].pickupSecondary = InputControl.setKey("Pick Up Secondary Gamepiece", KeyCode.X, new JoystickInput(JoystickButton.Button3, Joystick.Joystick1));
        buttons[0].releaseSecondary = InputControl.setKey("Release Secondary Gamepiece", KeyCode.E, new JoystickInput(JoystickButton.Button4, Joystick.Joystick1));
        buttons[0].spawnSecondary = InputControl.setKey("Spawn Secondary Gamepiece", KeyCode.Q, new JoystickInput(JoystickButton.Button5, Joystick.Joystick1));

        //Set axes
        //axes[0].horizontal = InputControl.setAxis("Joystick 1 Axis 2", buttons[0].left, buttons[0].right);
        //axes[0].vertical = InputControl.setAxis("Joystick 1 Axis 4", buttons[0].backward, buttons[0].forward);
        #endregion

        #region Player 2 Controls
        //Tank Controls
        buttons[1].tankFrontLeft = InputControl.setKey("2: Tank Front Left", new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick2));
        buttons[1].tankBackLeft = InputControl.setKey("2: Tank Back Left", new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick2));
        buttons[1].tankFrontRight = InputControl.setKey("2: Tank Front Right", new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick2));
        buttons[1].tankBackRight = InputControl.setKey("2: Tank Back Right", new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick2));

        //Remaining PWM controls
        buttons[1].pwm2Plus = InputControl.setKey("2: PWM 2 Positive", new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick2));
        buttons[1].pwm2Neg = InputControl.setKey("2: PWM 2 Negative", new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick2));
        buttons[1].pwm3Plus = InputControl.setKey("2: PWM 3 Positive", new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick2));
        buttons[1].pwm3Neg = InputControl.setKey("2: PWM 3 Negative", new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick2));
        buttons[1].pwm4Plus = InputControl.setKey("2: PWM 4 Positive", new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick2));
        buttons[1].pwm4Neg = InputControl.setKey("2: PWM 4 Negative", new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick2));
        buttons[1].pwm5Plus = InputControl.setKey("2: PWM 5 Positive", new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick2));
        buttons[1].pwm5Neg = InputControl.setKey("2: PWM 5 Negative", new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick2));
        buttons[1].pwm6Plus = InputControl.setKey("2: PWM 6 Positive", new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick2));
        buttons[1].pwm6Neg = InputControl.setKey("2: PWM 6 Negative", new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick2));

        //Other Controls
        buttons[1].resetRobot = InputControl.setKey("2: Reset Robot", new JoystickInput(JoystickButton.Button8, Joystick.Joystick2));
        buttons[1].cameraToggle = InputControl.setKey("2: Camera Toggle", new JoystickInput(JoystickButton.Button7, Joystick.Joystick2));
        buttons[1].pickupPrimary = InputControl.setKey("2: Pick Up Primary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick2));
        buttons[1].releasePrimary = InputControl.setKey("2: Release Primary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick2));
        buttons[1].spawnPrimary = InputControl.setKey("2: Spawn Primary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick2));
        buttons[1].pickupSecondary = InputControl.setKey("2: Pick Up Secondary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick2));
        buttons[1].releaseSecondary = InputControl.setKey("2: Release Secondary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick2));
        buttons[1].spawnSecondary = InputControl.setKey("2: Spawn Secondary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick2));
        #endregion

        #region Player 3 Controls
        //Tank Controls
        buttons[2].tankFrontLeft = InputControl.setKey("3: Tank Front Left", new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick3));
        buttons[2].tankBackLeft = InputControl.setKey("3: Tank Back Left", new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick3));
        buttons[2].tankFrontRight = InputControl.setKey("3: Tank Front Right", new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick3));
        buttons[2].tankBackRight = InputControl.setKey("3: Tank Back Right", new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick3));

        //Remaining PWM controls
        buttons[2].pwm2Plus = InputControl.setKey("3: PWM 2 Positive", new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick3));
        buttons[2].pwm2Neg = InputControl.setKey("3: PWM 2 Negative", new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick3));
        buttons[2].pwm3Plus = InputControl.setKey("3: PWM 3 Positive", new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick3));
        buttons[2].pwm3Neg = InputControl.setKey("3: PWM 3 Negative", new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick3));
        buttons[2].pwm4Plus = InputControl.setKey("3: PWM 4 Positive", new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick3));
        buttons[2].pwm4Neg = InputControl.setKey("3: PWM 4 Negative", new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick3));
        buttons[2].pwm5Plus = InputControl.setKey("3: PWM 5 Positive", new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick3));
        buttons[2].pwm5Neg = InputControl.setKey("3: PWM 5 Negative", new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick3));
        buttons[2].pwm6Plus = InputControl.setKey("3: PWM 6 Positive", new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick3));
        buttons[2].pwm6Neg = InputControl.setKey("3: PWM 6 Negative", new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick3));

        //Other Controls
        buttons[2].resetRobot = InputControl.setKey("3: Reset Robot", new JoystickInput(JoystickButton.Button8, Joystick.Joystick3));
        buttons[2].cameraToggle = InputControl.setKey("3: Camera Toggle", new JoystickInput(JoystickButton.Button7, Joystick.Joystick3));
        buttons[2].pickupPrimary = InputControl.setKey("3: Pick Up Primary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick3));
        buttons[2].releasePrimary = InputControl.setKey("3: Release Primary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick3));
        buttons[2].spawnPrimary = InputControl.setKey("3: Spawn Primary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick3));
        buttons[2].pickupSecondary = InputControl.setKey("3: Pick Up Secondary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick3));
        buttons[2].releaseSecondary = InputControl.setKey("3: Release Secondary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick3));
        buttons[2].spawnSecondary = InputControl.setKey("3: Spawn Secondary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick3));
        #endregion

        #region Player 4 Controls
        //Tank Controls
        buttons[3].tankFrontLeft = InputControl.setKey("4: Tank Front Left", new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick4));
        buttons[3].tankBackLeft = InputControl.setKey("4: Tank Back Left", new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick4));
        buttons[3].tankFrontRight = InputControl.setKey("4: Tank Front Right", new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick4));
        buttons[3].tankBackRight = InputControl.setKey("4: Tank Back Right", new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick4));

        //Remaining PWM controls
        buttons[3].pwm2Plus = InputControl.setKey("4: PWM 2 Positive", new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick4));
        buttons[3].pwm2Neg = InputControl.setKey("4: PWM 2 Negative", new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick4));
        buttons[3].pwm3Plus = InputControl.setKey("4: PWM 3 Positive", new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick4));
        buttons[3].pwm3Neg = InputControl.setKey("4: PWM 3 Negative", new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick4));
        buttons[3].pwm4Plus = InputControl.setKey("4: PWM 4 Positive", new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick4));
        buttons[3].pwm4Neg = InputControl.setKey("4: PWM 4 Negative", new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick4));
        buttons[3].pwm5Plus = InputControl.setKey("4: PWM 5 Positive", new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick4));
        buttons[3].pwm5Neg = InputControl.setKey("4: PWM 5 Negative", new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick4));
        buttons[3].pwm6Plus = InputControl.setKey("4: PWM 6 Positive", new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick4));
        buttons[3].pwm6Neg = InputControl.setKey("4: PWM 6 Negative", new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick4));

        //Other Controls
        buttons[3].resetRobot = InputControl.setKey("4: Reset Robot", new JoystickInput(JoystickButton.Button8, Joystick.Joystick4));
        buttons[3].cameraToggle = InputControl.setKey("4: Camera Toggle", new JoystickInput(JoystickButton.Button7, Joystick.Joystick4));
        buttons[3].pickupPrimary = InputControl.setKey("4: Pick Up Primary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick4));
        buttons[3].releasePrimary = InputControl.setKey("4: Release Primary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick4));
        buttons[3].spawnPrimary = InputControl.setKey("4: Spawn Primary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick4));
        buttons[3].pickupSecondary = InputControl.setKey("4: Pick Up Secondary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick4));
        buttons[3].releaseSecondary = InputControl.setKey("4: Release Secondary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick4));
        buttons[3].spawnSecondary = InputControl.setKey("4: Spawn Secondary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick4));
        #endregion

        #region Player 5 Controls
        //Tank Controls
        buttons[4].tankFrontLeft = InputControl.setKey("5: Tank Front Left", new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick5));
        buttons[4].tankBackLeft = InputControl.setKey("5: Tank Back Left", new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick5));
        buttons[4].tankFrontRight = InputControl.setKey("5: Tank Front Right", new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick5));
        buttons[4].tankBackRight = InputControl.setKey("5: Tank Back Right", new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick5));

        //Remaining PWM controls
        buttons[4].pwm2Plus = InputControl.setKey("5: PWM 2 Positive", new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick5));
        buttons[4].pwm2Neg = InputControl.setKey("5: PWM 2 Negative", new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick5));
        buttons[4].pwm3Plus = InputControl.setKey("5: PWM 3 Positive", new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick5));
        buttons[4].pwm3Neg = InputControl.setKey("5: PWM 3 Negative", new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick5));
        buttons[4].pwm4Plus = InputControl.setKey("5: PWM 4 Positive", new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick5));
        buttons[4].pwm4Neg = InputControl.setKey("5: PWM 4 Negative", new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick5));
        buttons[4].pwm5Plus = InputControl.setKey("5: PWM 5 Positive", new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick5));
        buttons[4].pwm5Neg = InputControl.setKey("5: PWM 5 Negative", new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick5));
        buttons[4].pwm6Plus = InputControl.setKey("5: PWM 6 Positive", new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick5));
        buttons[4].pwm6Neg = InputControl.setKey("5: PWM 6 Negative", new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick5));

        //Other Controls
        buttons[4].resetRobot = InputControl.setKey("5: Reset Robot", new JoystickInput(JoystickButton.Button8, Joystick.Joystick5));
        buttons[4].cameraToggle = InputControl.setKey("5: Camera Toggle", new JoystickInput(JoystickButton.Button7, Joystick.Joystick5));
        buttons[4].pickupPrimary = InputControl.setKey("5: Pick Up Primary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick5));
        buttons[4].releasePrimary = InputControl.setKey("5: Release Primary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick5));
        buttons[4].spawnPrimary = InputControl.setKey("5: Spawn Primary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick5));
        buttons[4].pickupSecondary = InputControl.setKey("5: Pick Up Secondary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick5));
        buttons[4].releaseSecondary = InputControl.setKey("5: Release Secondary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick5));
        buttons[4].spawnSecondary = InputControl.setKey("5: Spawn Secondary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick5));
        #endregion

        #region Player 6 Controls
        //Tank Controls
        buttons[5].tankFrontLeft = InputControl.setKey("6: Tank Front Left", new JoystickInput(JoystickAxis.Axis9Negative, Joystick.Joystick6));
        buttons[5].tankBackLeft = InputControl.setKey("6: Tank Back Left", new JoystickInput(JoystickAxis.Axis9Positive, Joystick.Joystick6));
        buttons[5].tankFrontRight = InputControl.setKey("6: Tank Front Right", new JoystickInput(JoystickAxis.Axis10Negative, Joystick.Joystick6));
        buttons[5].tankBackRight = InputControl.setKey("6: Tank Back Right", new JoystickInput(JoystickAxis.Axis10Positive, Joystick.Joystick6));

        //Remaining PWM controls
        buttons[5].pwm2Plus = InputControl.setKey("6: PWM 2 Positive", new JoystickInput(JoystickAxis.Axis3Positive, Joystick.Joystick6));
        buttons[5].pwm2Neg = InputControl.setKey("6: PWM 2 Negative", new JoystickInput(JoystickAxis.Axis3Negative, Joystick.Joystick6));
        buttons[5].pwm3Plus = InputControl.setKey("6: PWM 3 Positive", new JoystickInput(JoystickAxis.Axis5Positive, Joystick.Joystick6));
        buttons[5].pwm3Neg = InputControl.setKey("6: PWM 3 Negative", new JoystickInput(JoystickAxis.Axis5Negative, Joystick.Joystick6));
        buttons[5].pwm4Plus = InputControl.setKey("6: PWM 4 Positive", new JoystickInput(JoystickAxis.Axis6Positive, Joystick.Joystick6));
        buttons[5].pwm4Neg = InputControl.setKey("6: PWM 4 Negative", new JoystickInput(JoystickAxis.Axis6Negative, Joystick.Joystick6));
        buttons[5].pwm5Plus = InputControl.setKey("6: PWM 5 Positive", new JoystickInput(JoystickAxis.Axis7Positive, Joystick.Joystick6));
        buttons[5].pwm5Neg = InputControl.setKey("6: PWM 5 Negative", new JoystickInput(JoystickAxis.Axis7Negative, Joystick.Joystick6));
        buttons[5].pwm6Plus = InputControl.setKey("6: PWM 6 Positive", new JoystickInput(JoystickAxis.Axis8Positive, Joystick.Joystick6));
        buttons[5].pwm6Neg = InputControl.setKey("6: PWM 6 Negative", new JoystickInput(JoystickAxis.Axis8Negative, Joystick.Joystick6));

        //Other Controls
        buttons[5].resetRobot = InputControl.setKey("6: Reset Robot", new JoystickInput(JoystickButton.Button8, Joystick.Joystick6));
        buttons[5].cameraToggle = InputControl.setKey("6: Camera Toggle", new JoystickInput(JoystickButton.Button7, Joystick.Joystick6));
        buttons[5].pickupPrimary = InputControl.setKey("6: Pick Up Primary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick6));
        buttons[5].releasePrimary = InputControl.setKey("6: Release Primary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick6));
        buttons[5].spawnPrimary = InputControl.setKey("6: Spawn Primary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick6));
        buttons[5].pickupSecondary = InputControl.setKey("6: Pick Up Secondary Gamepiece", new JoystickInput(JoystickButton.Button3, Joystick.Joystick6));
        buttons[5].releaseSecondary = InputControl.setKey("6: Release Secondary Gamepiece", new JoystickInput(JoystickButton.Button4, Joystick.Joystick6));
        buttons[5].spawnSecondary = InputControl.setKey("6: Spawn Secondary Gamepiece", new JoystickInput(JoystickButton.Button5, Joystick.Joystick6));
        #endregion

        IsTankDrive = true;
        if (AuxFunctions.FindObject("SettingsMode") != null)
        {
            GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
            Debug.Log("read");
            GameObject.Find("Content").GetComponent<CreateButton>().UpdateButtons();
        }
        //GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
    }

    

    public static void RemoveTankKeys()
    {
        #region Remove Primary Controls
        //Basic robot controls
        InputControl.removeKey(buttons[0].tankFrontLeft);
        InputControl.removeKey(buttons[0].tankFrontRight);
        InputControl.removeKey(buttons[0].tankBackLeft);
        InputControl.removeKey(buttons[0].tankBackRight);

        //Remaining PWM controls
        InputControl.removeKey(buttons[0].pwm2Plus);
        InputControl.removeKey(buttons[0].pwm2Neg);
        InputControl.removeKey(buttons[0].pwm3Plus);
        InputControl.removeKey(buttons[0].pwm3Neg);
        InputControl.removeKey(buttons[0].pwm4Plus);
        InputControl.removeKey(buttons[0].pwm4Neg);
        InputControl.removeKey(buttons[0].pwm5Plus);
        InputControl.removeKey(buttons[0].pwm5Neg);
        InputControl.removeKey(buttons[0].pwm6Plus);
        InputControl.removeKey(buttons[0].pwm6Neg);

        //Other controls
        InputControl.removeKey(buttons[0].resetRobot);
        InputControl.removeKey(buttons[0].cameraToggle);
        InputControl.removeKey(buttons[0].pickupPrimary);
        InputControl.removeKey(buttons[0].releasePrimary);
        InputControl.removeKey(buttons[0].spawnPrimary);
        InputControl.removeKey(buttons[0].pickupSecondary);
        InputControl.removeKey(buttons[0].releaseSecondary);
        InputControl.removeKey(buttons[0].spawnSecondary);

        #endregion

        #region Player 2 Controls
        //Basic robot controls
        InputControl.removeKey(buttons[1].tankFrontLeft);
        InputControl.removeKey(buttons[1].tankFrontRight);
        InputControl.removeKey(buttons[1].tankBackLeft);
        InputControl.removeKey(buttons[1].tankBackRight);

        //Remaining PWM controls
        InputControl.removeKey(buttons[1].pwm2Plus);
        InputControl.removeKey(buttons[1].pwm2Neg);
        InputControl.removeKey(buttons[1].pwm3Plus);
        InputControl.removeKey(buttons[1].pwm3Neg);
        InputControl.removeKey(buttons[1].pwm4Plus);
        InputControl.removeKey(buttons[1].pwm4Neg);
        InputControl.removeKey(buttons[1].pwm5Plus);
        InputControl.removeKey(buttons[1].pwm5Neg);
        InputControl.removeKey(buttons[1].pwm6Plus);
        InputControl.removeKey(buttons[1].pwm6Neg);

        //Other controls
        InputControl.removeKey(buttons[1].resetRobot);
        InputControl.removeKey(buttons[1].cameraToggle);
        InputControl.removeKey(buttons[1].pickupPrimary);
        InputControl.removeKey(buttons[1].releasePrimary);
        InputControl.removeKey(buttons[1].spawnPrimary);
        InputControl.removeKey(buttons[1].pickupSecondary);
        InputControl.removeKey(buttons[1].releaseSecondary);
        InputControl.removeKey(buttons[1].spawnSecondary);
        #endregion

        #region Player 3 Controls
        //Basic robot controls
        InputControl.removeKey(buttons[2].tankFrontLeft);
        InputControl.removeKey(buttons[2].tankFrontRight);
        InputControl.removeKey(buttons[2].tankBackLeft);
        InputControl.removeKey(buttons[2].tankBackRight);

        //Remaining PWM controls
        InputControl.removeKey(buttons[2].pwm2Plus);
        InputControl.removeKey(buttons[2].pwm2Neg);
        InputControl.removeKey(buttons[2].pwm3Plus);
        InputControl.removeKey(buttons[2].pwm3Neg);
        InputControl.removeKey(buttons[2].pwm4Plus);
        InputControl.removeKey(buttons[2].pwm4Neg);
        InputControl.removeKey(buttons[2].pwm5Plus);
        InputControl.removeKey(buttons[2].pwm5Neg);
        InputControl.removeKey(buttons[2].pwm6Plus);
        InputControl.removeKey(buttons[2].pwm6Neg);

        //Other controls
        InputControl.removeKey(buttons[2].resetRobot);
        InputControl.removeKey(buttons[2].cameraToggle);
        InputControl.removeKey(buttons[2].pickupPrimary);
        InputControl.removeKey(buttons[2].releasePrimary);
        InputControl.removeKey(buttons[2].spawnPrimary);
        InputControl.removeKey(buttons[2].pickupSecondary);
        InputControl.removeKey(buttons[2].releaseSecondary);
        InputControl.removeKey(buttons[2].spawnSecondary);
        #endregion

        #region Player4 Controls
        //Basic robot controls
        InputControl.removeKey(buttons[3].tankFrontLeft);
        InputControl.removeKey(buttons[3].tankFrontRight);
        InputControl.removeKey(buttons[3].tankBackLeft);
        InputControl.removeKey(buttons[3].tankBackRight);

        //Remaining PWM controls
        InputControl.removeKey(buttons[3].pwm2Plus);
        InputControl.removeKey(buttons[3].pwm2Neg);
        InputControl.removeKey(buttons[3].pwm3Plus);
        InputControl.removeKey(buttons[3].pwm3Neg);
        InputControl.removeKey(buttons[3].pwm4Plus);
        InputControl.removeKey(buttons[3].pwm4Neg);
        InputControl.removeKey(buttons[3].pwm5Plus);
        InputControl.removeKey(buttons[3].pwm5Neg);
        InputControl.removeKey(buttons[3].pwm6Plus);
        InputControl.removeKey(buttons[3].pwm6Neg);

        //Other controls
        InputControl.removeKey(buttons[3].resetRobot);
        InputControl.removeKey(buttons[3].cameraToggle);
        InputControl.removeKey(buttons[3].pickupPrimary);
        InputControl.removeKey(buttons[3].releasePrimary);
        InputControl.removeKey(buttons[3].spawnPrimary);
        InputControl.removeKey(buttons[3].pickupSecondary);
        InputControl.removeKey(buttons[3].releaseSecondary);
        InputControl.removeKey(buttons[3].spawnSecondary);
        #endregion

        #region Player 5 Controls
        //Basic robot controls
        InputControl.removeKey(buttons[4].tankFrontLeft);
        InputControl.removeKey(buttons[4].tankFrontRight);
        InputControl.removeKey(buttons[4].tankBackLeft);
        InputControl.removeKey(buttons[4].tankBackRight);

        //Remaining PWM controls
        InputControl.removeKey(buttons[4].pwm2Plus);
        InputControl.removeKey(buttons[4].pwm2Neg);
        InputControl.removeKey(buttons[4].pwm3Plus);
        InputControl.removeKey(buttons[4].pwm3Neg);
        InputControl.removeKey(buttons[4].pwm4Plus);
        InputControl.removeKey(buttons[4].pwm4Neg);
        InputControl.removeKey(buttons[4].pwm5Plus);
        InputControl.removeKey(buttons[4].pwm5Neg);
        InputControl.removeKey(buttons[4].pwm6Plus);
        InputControl.removeKey(buttons[4].pwm6Neg);

        //Other controls
        InputControl.removeKey(buttons[4].resetRobot);
        InputControl.removeKey(buttons[4].cameraToggle);
        InputControl.removeKey(buttons[4].pickupPrimary);
        InputControl.removeKey(buttons[4].releasePrimary);
        InputControl.removeKey(buttons[4].spawnPrimary);
        InputControl.removeKey(buttons[4].pickupSecondary);
        InputControl.removeKey(buttons[4].releaseSecondary);
        InputControl.removeKey(buttons[4].spawnSecondary);
        #endregion

        #region Player 6 Controls
        //Basic robot controls
        InputControl.removeKey(buttons[5].tankFrontLeft);
        InputControl.removeKey(buttons[5].tankFrontRight);
        InputControl.removeKey(buttons[5].tankBackLeft);
        InputControl.removeKey(buttons[5].tankBackRight);

        //Remaining PWM controls
        InputControl.removeKey(buttons[5].pwm2Plus);
        InputControl.removeKey(buttons[5].pwm2Neg);
        InputControl.removeKey(buttons[5].pwm3Plus);
        InputControl.removeKey(buttons[5].pwm3Neg);
        InputControl.removeKey(buttons[5].pwm4Plus);
        InputControl.removeKey(buttons[5].pwm4Neg);
        InputControl.removeKey(buttons[5].pwm5Plus);
        InputControl.removeKey(buttons[5].pwm5Neg);
        InputControl.removeKey(buttons[5].pwm6Plus);
        InputControl.removeKey(buttons[5].pwm6Neg);

        //Other controls
        InputControl.removeKey(buttons[5].resetRobot);
        InputControl.removeKey(buttons[5].cameraToggle);
        InputControl.removeKey(buttons[5].pickupPrimary);
        InputControl.removeKey(buttons[5].releasePrimary);
        InputControl.removeKey(buttons[5].spawnPrimary);
        InputControl.removeKey(buttons[5].pickupSecondary);
        InputControl.removeKey(buttons[5].releaseSecondary);
        InputControl.removeKey(buttons[5].spawnSecondary);
        #endregion

        //GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
    }

    /// <summary>
    /// Converts string representation of CustomInput to CustomInput.
    /// </summary>
    /// <returns>CustomInput from string.</returns>
    /// <param name="value">String representation of CustomInput.</param>
    private static CustomInput customInputFromString(string value)
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