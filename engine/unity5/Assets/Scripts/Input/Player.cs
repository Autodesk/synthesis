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
            // TankJoystick,
            TankKeyboard
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
        private Dictionary<string, KeyMapping> arcadeDriveMap = new Dictionary<string, KeyMapping>();

        //Set of tank drive keys
        private List<KeyMapping> tankDriveList = new List<KeyMapping>();
        private Dictionary<string, KeyMapping> tankDriveMap = new Dictionary<string, KeyMapping>();

        //Set of tank drive defaults
        private List<KeyMapping> resetTankDriveList = new List<KeyMapping>();
        private Dictionary<string, KeyMapping> resetTankDriveMap = new Dictionary<string, KeyMapping>();

        //Set of arcade drive defaults
        private List<KeyMapping> resetArcadeDriveList = new List<KeyMapping>();
        private Dictionary<string, KeyMapping> resetArcadeDriveMap = new Dictionary<string, KeyMapping>();

        //Set of arcade drive axes
        private List<Axis> arcadeAxesList = new List<Axis>();
        private Dictionary<string, Axis> arcadeAxesMap = new Dictionary<string, Axis>();

        //Set of tank drive axes
        private Dictionary<string, Axis> tankAxesMap = new Dictionary<string, Axis>();
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
                    if (arcadeDriveMap.TryGetValue(name, out outKey) && resetArcadeDriveMap.TryGetValue(name, out outKey))
                    {
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

                        //Assigns each key map with the correct name and key (dependent on the list)
                        arcadeDriveMap.Add(name, outKey);
                        resetArcadeDriveMap.Add(name, defaultKey);
                    }
                    break;
                case ControlProfile.TankKeyboard:
                    if (tankDriveMap.TryGetValue(name, out outKey) && resetTankDriveMap.TryGetValue(name, out outKey))
                    {
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

                        //Assigns each key map with the correct name and key (dependent on the list)
                        tankDriveMap.Add(name, outKey);
                        resetTankDriveMap.Add(name, defaultKey);
                    }
                    break;
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
                    if (arcadeAxesMap.TryGetValue(name, out outAxis))
                    {
                        outAxis.set(negative, positive);
                    }
                    else
                    {
                        outAxis = new Axis(name, negative, positive);

                        arcadeAxesList.Add(outAxis);
                        arcadeAxesMap.Add(name, outAxis);
                    }
                    break;
                case ControlProfile.TankKeyboard:
                    if (tankAxesMap.TryGetValue(name, out outAxis))
                    {
                        outAxis.set(negative, positive);
                    }
                    else
                    {
                        outAxis = new Axis(name, negative, positive);

                        tankAxesList.Add(outAxis);
                        tankAxesMap.Add(name, outAxis);
                    }
                    break;
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
                if (activeList[i].name.Contains(" Pick Up ") || activeList[i].name.Contains(" Release ") || activeList[i].name.Contains(" Spawn "))
                    if (FieldDataHandler.gamepieces.Where(g => activeList[i].name.Contains(g.name)).ToArray().Count() == 0)
                    {
                        if (controlProfile == ControlProfile.ArcadeKeyboard) { arcadeDriveMap.Remove(activeList[i].name); resetArcadeDriveMap.Remove(activeList[i].name); }
                        else { tankDriveMap.Remove(activeList[i].name); resetTankDriveMap.Remove(activeList[i].name); }
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
            controlProfile = ControlProfile.TankKeyboard;
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
