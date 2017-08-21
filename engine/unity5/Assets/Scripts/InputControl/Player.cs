using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using UnityEngine.UI;

//=========================================================================================
//                                      Player Class
// Description: Controls the individual player's controls through KeyMapping Lists and Maps
// Adapted from: https://github.com/Gris87/InputControl
//=========================================================================================
public class Player
{
    public Player()
    {
        //Constructor; defaults the player to Arcade Drive
        activeList = arcadeDriveList;
    }

    // Use this for initialization
    void Start()
    {

    }

    //Checks if Tank Drive is enabled
    public bool isTankDrive;

    //The list called on the current player; set of current keys
    private List<KeyMapping> activeList;

    //Set of arcade drive keys
    private List<KeyMapping> arcadeDriveList = new List<KeyMapping>();
    private Dictionary<string, KeyMapping> arcadeDriveMap = new Dictionary<string, KeyMapping>();

    //Set of Tank Drive keys
    private List<KeyMapping> tankDriveList = new List<KeyMapping>();
    private Dictionary<string, KeyMapping> tankDriveMap = new Dictionary<string, KeyMapping>();

    private List<KeyMapping> resetTankDriveList = new List<KeyMapping>();
    private Dictionary<string, KeyMapping> resetTankDriveMap = new Dictionary<string, KeyMapping>();

    private List<KeyMapping> resetArcadeDriveList = new List<KeyMapping>();
    private Dictionary<string, KeyMapping> resetArcadeDriveMap = new Dictionary<string, KeyMapping>();

    //Set of arcade drive axes.
    private List<Axis> arcadeAxesList = new List<Axis>();
    private Dictionary<string, Axis> arcadeAxesMap = new Dictionary<string, Axis>();

    //Set of tank drive axes.
    private Dictionary<string, Axis> tankAxesMap = new Dictionary<string, Axis>();
    private List<Axis> tankAxesList = new List<Axis>();


    ///The SetKey() and SetAxis() Functions called here are specific to our Controls.cs initialization.
    ///Additional functions can be found in <see cref="InputControl"/>
    #region setKey() and setAxis() Functions
    /// <summary>
    /// Create new <see cref="KeyMapping"/> with specified name and inputs.
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
    /// Create new <see cref="KeyMapping"/> with specified name and inputs.
    /// </summary>
    /// <returns>Created KeyMapping.</returns>
    /// <param name="name">KeyMapping name.</param>
    /// <param name="primary">Primary input.</param>
    /// <param name="secondary">Secondary input.</param>
    /// <param name="third">Third input.</param>
    public KeyMapping setKey(string name, CustomInput primary = null, CustomInput secondary = null, bool isTankDrive = false)
    {
        KeyMapping outKey = null;

        if (!isTankDrive)
        {
            if (arcadeDriveMap.TryGetValue(name, out outKey) && resetArcadeDriveMap.TryGetValue(name, out outKey))
            {
                outKey.primaryInput = primary;
                outKey.secondaryInput = secondary;
            }
            else
            {
                outKey = new KeyMapping(name, primary, secondary);

                arcadeDriveList.Add(outKey);
                resetArcadeDriveList.Add(outKey);
                arcadeDriveMap.Add(name, outKey);
                resetArcadeDriveMap.Add(name, outKey);
            }
        }
        else
        {
            if (tankDriveMap.TryGetValue(name, out outKey) && resetTankDriveMap.TryGetValue(name, out outKey))
            {
                outKey.primaryInput = primary;
                outKey.secondaryInput = secondary;
            }
            else
            {
                outKey = new KeyMapping(name, primary, secondary);

                tankDriveList.Add(outKey);
                resetTankDriveList.Add(outKey);
                tankDriveMap.Add(name, outKey);
                resetTankDriveMap.Add(name, outKey);
            }
        }

        return outKey;
    }

    /// <summary>
    /// Create new <see cref="Axis"/> with specified negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/>.
    /// </summary>
    /// <returns>Created Axis.</returns>
    /// <param name="name">Axis name.</param>
    /// <param name="negative">Negative KeyMapping.</param>
    /// <param name="positive">Positive KeyMapping.</param>
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

    //public void SetAxis(List<KeyMapping> list, Dictionary<string, Axis> map)
    //{
    //    if (map.TryGetValue(name, out outAxis))
    //    {
    //        list.set(negative, positive);
    //    }
    //    else
    //    {
    //        outAxis = new Axis(name, negative, positive);

    //        TankAxesList.Add(outAxis);
    //        TankAxesMap.Add(name, outAxis);
    //    }
    //}
    #endregion

    #region argToInput Helper Functions for setKey() and setAxis()
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


    public ReadOnlyCollection<KeyMapping> GetTankList()
    {
        return tankDriveList.AsReadOnly();
    }

    public ReadOnlyCollection<KeyMapping> GetArcadeList()
    {
        return arcadeDriveList.AsReadOnly();
    }

    public ReadOnlyCollection<KeyMapping> GetTankDefaults()
    {
        return resetTankDriveList.AsReadOnly();
    }

    public ReadOnlyCollection<KeyMapping> GetArcadeDefaults()
    {
        return resetArcadeDriveList.AsReadOnly();
    }

    public ReadOnlyCollection<KeyMapping> GetActiveList()
    {
        return activeList.AsReadOnly();
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
    /// Sets the activeList to Tank Drive.
    /// </summary>
    public void SetTankDrive()
    {
        isTankDrive = true;
        activeList = tankDriveList;
    }

    /// <summary>
    /// Sets the activeList to Arcade Drive.
    /// </summary>
    public void SetArcadeDrive()
    {
        isTankDrive = false;
        activeList = arcadeDriveList;
    }

    public void ResetTank()
    {
        isTankDrive = true;
        Controls.TankDriveEnabled = true;
        activeList = resetTankDriveList;

    }

    public void ResetArcade()
    {
        isTankDrive = false;
        Controls.TankDriveEnabled = false;
        activeList = resetArcadeDriveList;
    }
}
