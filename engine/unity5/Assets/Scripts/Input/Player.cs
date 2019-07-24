using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using UnityEngine.UI;
using Synthesis.Input.Inputs;
using Synthesis.Field;
using System.Linq;

//=========================================================================================
//                                      Player Class
// Description: Controls the individual player's controls by generating individual lists
///             for each player through <see cref="KeyMapping"/>KeyMapping Lists and Maps
//=========================================================================================

namespace Synthesis.Input
{
    public class Player
    {
        public const int PLAYER_COUNT = 6;
        public enum ControlProfile // Order must correspond to that in the dropdown list within Unity
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

        public const ControlProfile DEFAULT_CONTROL_PROFILE = ControlProfile.ArcadeKeyboard;

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

                pwmPos = new KeyMapping[DriveJoints.PWM_COUNT - DriveJoints.PWM_OFFSET];
                for (var i = 0; i < pwmPos.Length; i++)
                    pwmPos[i] = new KeyMapping();

                pwmNeg = new KeyMapping[DriveJoints.PWM_COUNT - DriveJoints.PWM_OFFSET];
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

                pwmAxes = new Axis[DriveJoints.PWM_COUNT - DriveJoints.PWM_OFFSET];
            }
        }


        /// <summary>
        /// Set of buttons.
        /// </summary>
        public Buttons buttons;

        /// <summary>
        /// Set of axes.
        /// </summary>
        public Axes axes;

        public Player()
        {
            SetControlProfile(DEFAULT_CONTROL_PROFILE);
            activeList = arcadeDriveList;

            buttons = new Buttons();
            axes = new Axes();
        }

        //Checks if tank drive is enabled
        private ControlProfile activeControlProfile;

        //The list and controls called on the current player
        private List<KeyMapping> activeList;

        //Set of arcade drive keys
        private List<KeyMapping> arcadeDriveList = new List<KeyMapping>();

        //Set of tank drive keys
        private List<KeyMapping> tankDriveList = new List<KeyMapping>();

        //Set of tank drive defaults
        private List<KeyMapping> resetTankDriveList = new List<KeyMapping>();

        //Set of arcade drive defaults
        private List<KeyMapping> resetArcadeDriveList = new List<KeyMapping>();

        //Set of arcade drive axes
        private List<Axis> arcadeAxesList = new List<Axis>();

        //Set of tank drive axes
        private List<Axis> tankAxesList = new List<Axis>();

        //=================================================================================================
        ///The SetKey() and SetAxis() Functions called here are specific to our Controls.cs initialization.
        ///Additional functions (aside from Synthesis) can be found in <see cref="InputControl"/>
        ///Adapted from: https://github.com/Gris87/InputControl
        //=================================================================================================

        #region setKey() and SetAxis() Functions

        public KeyMapping SetKey(ControlProfile controlProfile, string name, JoystickInput primary = null)
        {
            return SetKey(controlProfile, name, argToInput(primary), null);
        }

        public KeyMapping SetKey(ControlProfile controlProfile, string name, KeyCode? primary = null)
        {
            return SetKey(controlProfile, name, argToInput(primary), null);
        }

        public KeyMapping SetKey(ControlProfile controlProfile, string name, KeyCode? primary = null, KeyCode? secondary = null)
        {
            return SetKey(controlProfile, name, argToInput(primary), argToInput(secondary));
        }

        /// <summary>
        /// Creates new <see cref="KeyMapping"/> with specified name, primary CustomInput, and secondary CustomInput.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="controlProfile">Profile to write to.</param>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public KeyMapping SetKey(ControlProfile controlProfile, string name, CustomInput primary = null, CustomInput secondary = null)
        {

            KeyMapping outKey = null; //Key to return
            KeyMapping defaultKey = null; //Key to store default key preferances (for resetting individual player lists)

            switch (controlProfile)
            {
                case ControlProfile.ArcadeKeyboard:
                    {
                        var key_i = arcadeDriveList.FindIndex(x => x.name == name);
                        var default_key_i = resetArcadeDriveList.FindIndex(x => x.name == name);
                        if (key_i != -1 && default_key_i != -1)
                        {
                            outKey = resetArcadeDriveList[default_key_i];
                            outKey.primaryInput = primary;
                            outKey.secondaryInput = secondary;
                        }
                        else
                        {
                            //Sets controls to the main key list (outKey) and the default list 
                            //(defaultKey stores controls (defaults) at initialization)
                            outKey = new KeyMapping(name, primary, secondary);
                            defaultKey = new KeyMapping(name, primary, secondary);

                            //Assigns each list with correct key
                            arcadeDriveList.Add(outKey);
                            resetArcadeDriveList.Add(defaultKey);
                        }
                        break;
                    }
                case ControlProfile.TankJoystick:
                    {
                        var key_i = tankDriveList.FindIndex(x => x.name == name);
                        var default_key_i = resetTankDriveList.FindIndex(x => x.name == name);
                        if (key_i != -1 && default_key_i != -1)
                        {
                            outKey = resetTankDriveList[default_key_i];
                            outKey.primaryInput = primary;
                            outKey.secondaryInput = secondary;
                        }
                        else
                        {
                            //Sets controls to the main key list (outKey) and the default list 
                            //(defaultKey stores controls (defaults) at initialization)
                            outKey = new KeyMapping(name, primary, secondary);
                            defaultKey = new KeyMapping(name, primary, secondary);

                            //Assigns each list with correct key
                            tankDriveList.Add(outKey);
                            resetTankDriveList.Add(defaultKey);
                        }
                        break;
                    }
                default:
                    throw new System.Exception("Unsupported control profile " + controlProfile);
            }
            return outKey;
        }

        /// <summary>
        /// Creates new <see cref="Axis"/> with specified negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/>.
        /// </summary>
        /// <returns>Created Axis.</returns>
        /// <param name="controlProfile">Profile to write to.</param>
        /// <param name="name">Axis name.</param>
        /// <param name="negative">Negative KeyMapping.</param>
        /// <param name="positive">Positive KeyMapping.</param>
        public Axis SetAxis(ControlProfile controlProfile, string name, KeyMapping negative, KeyMapping positive)
        {
            Axis outAxis = null;

            switch (controlProfile)
            {
                case ControlProfile.ArcadeKeyboard:
                    {
                        var key_i = arcadeAxesList.FindIndex(x => x.Name == name);
                        if (key_i != -1)
                        {
                            outAxis = arcadeAxesList[key_i];
                            outAxis.set(negative, positive);
                        }
                        else
                        {
                            outAxis = new Axis(name, negative, positive);

                            arcadeAxesList.Add(outAxis);
                        }
                        break;
                    }
                case ControlProfile.TankJoystick:
                    {
                        var key_i = tankAxesList.FindIndex(x => x.Name == name);
                        if (key_i != -1)
                        {
                            outAxis = tankAxesList[key_i];
                            outAxis.set(negative, positive);
                        }
                        else
                        {
                            outAxis = new Axis(name, negative, positive);

                            tankAxesList.Add(outAxis);
                        }
                        break;
                    }
                default:
                    throw new System.Exception("Unsupported control profile " + controlProfile);
            }
            return outAxis;
        }
        #endregion

        #region argToInput Helper Functions for setKey() and SetAxis()
        //Source: https://github.com/Gris87/InputControl
        /// <summary>
        /// Convert argument to <see cref="CustomInput"/>.
        /// </summary>
        /// <returns>Converted CustomInput.</returns>
        /// <param name="arg">Some kind of argument.</param>
        private static CustomInput argToInput(CustomInput arg)
        {
            return arg;
        }

        /// <summary>
        /// Convert argument to <see cref="CustomInput"/>.
        /// </summary>
        /// <returns>Converted CustomInput.</returns>
        /// <param name="arg">Some kind of argument.</param>
        private static CustomInput argToInput(KeyCode? arg)
        {
            if (arg == null)
                return null;
            return new KeyboardInput(arg.Value);
        }
        #endregion

        /// <summary>
        /// Gets the active player list.
        /// </summary>
        /// <returns>The active player list.</returns>
        public ReadOnlyCollection<KeyMapping> GetActiveList()
        {
            for(int i = 0; i < activeList.Count; i++)
            {
                if ((activeList[i].name.Contains(" Pick Up ") || activeList[i].name.Contains(" Release ") || activeList[i].name.Contains(" Spawn ")) &&
                    FieldDataHandler.gamepieces.Where(g => activeList[i].name.Contains(g.name)).ToArray().Count() == 0)
                {
                    switch (activeControlProfile) // Remove gamepiece controls when field changes // TODO only do when field changes
                    {
                        case ControlProfile.ArcadeKeyboard:
                            arcadeDriveList.Remove(activeList[i]);
                            resetArcadeDriveList.Remove(activeList[i]);
                            break;
                        case ControlProfile.TankJoystick:
                            tankDriveList.Remove(activeList[i]);
                            resetTankDriveList.Remove(activeList[i]);
                            break;
                        default:
                            throw new System.Exception("Unsupported control profile");
                    }
                    activeList.RemoveAt(i);
                }
            }
            return activeList.AsReadOnly();
        }

        /// <summary>
        /// Gets the set of tank drive keys.
        /// </summary>
        /// <returns>The set of tank drive keys.</returns>
        public ReadOnlyCollection<KeyMapping> GetTankList()
        {
            return tankDriveList.AsReadOnly();
        }

        /// <summary>
        /// Gets the set of arcade drive keys.
        /// </summary>
        /// <returns>The set of arcade drive keys.</returns>
        public ReadOnlyCollection<KeyMapping> GetArcadeList()
        {
            return arcadeDriveList.AsReadOnly();
        }

        /// <summary>
        /// Gets the set of tank drive default keys.
        /// </summary>
        /// <returns>The set of tank drive default keys.</returns>
        public ReadOnlyCollection<KeyMapping> GetTankDefaults()
        {
            return resetTankDriveList.AsReadOnly();
        }

        /// <summary>
        /// Gets the set of arcade drive default keys.
        /// </summary>
        /// <returns>The set of arcade drive default keys.</returns>
        public ReadOnlyCollection<KeyMapping> GetArcadeDefaults()
        {
            return resetArcadeDriveList.AsReadOnly();
        }

        public ReadOnlyCollection<Axis> GetTankAxesList()
        {
            return tankAxesList.AsReadOnly();
        }

        public ReadOnlyCollection<Axis> GetArcadeAxesList()
        {
            return arcadeAxesList.AsReadOnly();
        }

        public void SetControlProfile(ControlProfile p)
        {
            activeControlProfile = p;
            switch (activeControlProfile)
            {
                case ControlProfile.ArcadeKeyboard:
                    activeList = arcadeDriveList;
                    break;
                case ControlProfile.TankJoystick:
                    activeList = tankDriveList;
                    break;
                default:
                    throw new System.Exception("Unsupported control profile");
            }
        }

        public ControlProfile GetActiveControlProfile()
        {
            return activeControlProfile;
        }

        /// <summary>
        /// Resets the activeList to tank drive defaults.
        /// </summary>
        public void ResetTank()
        {
            foreach (KeyMapping defaultKey in resetTankDriveList)
            {
                foreach (KeyMapping key in tankDriveList)
                {
                    if (key.name.Equals(defaultKey.name))
                    {
                        key.set(defaultKey);
                    }
                }
            }
            SetControlProfile(ControlProfile.TankJoystick);
        }

        /// <summary>
        /// Resets the activeList to arcade drive defaults.
        /// </summary>
        public void ResetArcade()
        {
            foreach (KeyMapping defaultKey in resetArcadeDriveList)
            {
                foreach (KeyMapping key in arcadeDriveList)
                {
                    if (key.name.Equals(defaultKey.name))
                    {
                        key.set(defaultKey);
                    }
                }
            }
            SetControlProfile(ControlProfile.ArcadeKeyboard);
        }
    }
}
