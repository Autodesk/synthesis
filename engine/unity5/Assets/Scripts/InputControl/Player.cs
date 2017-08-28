using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using UnityEngine.UI;

//=========================================================================================
//                                      Player Class
// Description: Controls the individual player's controls by generating individual lists
///             for each player through <see cref="KeyMapping"/>KeyMapping Lists and Maps
//=========================================================================================
public class Player
{
    public Player()
    {
        //Constructor: defaults the active player to Arcade Drive
        activeList = arcadeDriveList;
    }

    // Use this for initialization
    void Start()
    {

    }

    //Checks if tank drive is enabled
    public bool isTankDrive;

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

    #region setKey() and setAxis() Functions
    /// <summary>
    /// Creates new <see cref="KeyMapping"/> with specified name, primary KeyCode, and secondary CustomInput.
    /// </summary>
    /// <returns>Created KeyMapping.</returns>
    /// <param name="name">KeyMapping name.</param>
    /// <param name="primary">Primary input.</param>
    /// <param name="secondary">Secondary input.</param>
    public KeyMapping setKey(string name, KeyCode primary, CustomInput secondary)
    {
        return setKey(name, argToInput(primary), argToInput(secondary));
    }

    /// <summary>
    /// Creates new <see cref="KeyMapping"/> with specified name, primary CustomInput, and secondary CustomInput.
    /// </summary>
    /// <returns>Created KeyMapping.</returns>
    /// <param name="name">KeyMapping name.</param>
    /// <param name="primary">Primary input.</param>
    /// <param name="secondary">Secondary input.</param>
    /// <param name="isTankDrive">Boolean to check if TankDrive is active.</param>
    public KeyMapping setKey(string name, CustomInput primary = null, CustomInput secondary = null, bool isTankDrive = false)
    {
        KeyMapping outKey = null; //Key to return
        KeyMapping defaultKey = null; //Key to store default key preferances (for resetting individual player lists)

        if (!isTankDrive) //Arcade Drive Enabled
        {
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
        }
        else //Tank Drive Enabled
        {
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
        }
        return outKey;
    }

    /// <summary>
    /// Creates new <see cref="Axis"/> with specified negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/>.
    /// </summary>
    /// <returns>Created Axis.</returns>
    /// <param name="name">Axis name.</param>
    /// <param name="negative">Negative KeyMapping.</param>
    /// <param name="positive">Positive KeyMapping.</param>
    /// <param name="isTankDrive">Boolean to check if TankDrive is active.</param>
    public Axis setAxis(string name, KeyMapping negative, KeyMapping positive, bool isTankDrive = false)
    {
        Axis outAxis = null;

        if (!isTankDrive)
        {
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
        }
        else
        {
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
        }
        return outAxis;
    }
    #endregion

    #region argToInput Helper Functions for setKey() and setAxis()
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
    private static CustomInput argToInput(KeyCode arg)
    {
        return new KeyboardInput(arg);
    }
    #endregion

    /// <summary>
    /// Gets the active player list.
    /// </summary>
    /// <returns>The active player list.</returns>
    public ReadOnlyCollection<KeyMapping> GetActiveList()
    {
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
        isTankDrive = true;
        activeList = tankDriveList;
    }

    /// <summary>
    /// Sets the activeList to arcade drive.
    /// </summary>
    public void SetArcadeDrive()
    {
        isTankDrive = false;
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
        isTankDrive = true;
        activeList = tankDriveList;
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
        isTankDrive = false;
        activeList = arcadeDriveList;
    }
}