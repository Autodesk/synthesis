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

        public Player()
        {
            //Constructor: defaults the active player to Arcade Drive
            controlProfile = DEFAULT_CONTROL_PROFILE;
            activeList = arcadeDriveList;
        }

        //Checks if tank drive is enabled
        public ControlProfile controlProfile;

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
                    switch (controlProfile) // Remove gamepiece controls when field changes
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

        /// <summary>
        /// Sets the activeList to tank drive.
        /// </summary>
        public void SetTankDrive()
        {
            controlProfile = ControlProfile.TankJoystick;
            activeList = tankDriveList;
        }

        /// <summary>
        /// Sets the activeList to arcade drive.
        /// </summary>
        public void SetArcadeDrive()
        {
            controlProfile = ControlProfile.ArcadeKeyboard;
            activeList = arcadeDriveList;
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
                        key.primaryInput = defaultKey.primaryInput;
                        key.secondaryInput = defaultKey.secondaryInput;
                    }
                }
            }
            SetTankDrive();
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
                        key.primaryInput = defaultKey.primaryInput;
                        key.secondaryInput = defaultKey.secondaryInput;
                    }
                }
            }
            SetArcadeDrive();
        }
    }
}
