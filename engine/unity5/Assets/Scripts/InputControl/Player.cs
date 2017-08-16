using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public Player()
    {
        activeList = arcadeDriveList;
    }
	// Use this for initialization
	void Start ()
    {
		
	}

    public bool isTankDrive;

    private List<KeyMapping> activeList;
    private List<KeyMapping> arcadeDriveList = new List<KeyMapping>();
    private List<KeyMapping> tankDriveList = new List<KeyMapping>();

    private Dictionary<string, KeyMapping> arcadeDriveMap = new Dictionary<string, KeyMapping>();
    private Dictionary<string, KeyMapping> tankDriveMap = new Dictionary<string, KeyMapping>();

    private List<Axis> arcadeAxesList = new List<Axis>();
    private List<Axis> tankAxesList = new List<Axis>();

    private Dictionary<string, Axis> arcadeAxesMap = new Dictionary<string, Axis>();
    private Dictionary<string, Axis> tankAxesMap = new Dictionary<string, Axis>();

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
            if (arcadeDriveMap.TryGetValue(name, out outKey))
            {
                outKey.primaryInput = primary;
                outKey.secondaryInput = secondary;
            }
            else
            {
                outKey = new KeyMapping(name, primary, secondary);

                arcadeDriveList.Add(outKey);
                arcadeDriveMap.Add(name, outKey);
            }
        }
        else
        {
            if (tankDriveMap.TryGetValue(name, out outKey))
            {
                outKey.primaryInput = primary;
                outKey.secondaryInput = secondary;
            }
            else
            {
                outKey = new KeyMapping(name, primary, secondary);

                tankDriveList.Add(outKey);
                tankDriveMap.Add(name, outKey);
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

    public ReadOnlyCollection<KeyMapping> GetTankList()
    {
        return tankDriveList.AsReadOnly();
    }

    public ReadOnlyCollection<KeyMapping> GetArcadeList()
    {
        return arcadeDriveList.AsReadOnly();
    }

    public ReadOnlyCollection<Axis> GetTankAxesList()
    {
        return tankAxesList.AsReadOnly();
    }

    public ReadOnlyCollection<Axis> GetArcadeAxesList()
    {
        return arcadeAxesList.AsReadOnly();
    }

    public ReadOnlyCollection<KeyMapping> GetActiveList()
    {
        return activeList.AsReadOnly();
    }

    public void SetTankDrive()
    {
        isTankDrive = true;
        activeList = tankDriveList;
    }

    public void SetArcadeDrive()
    {
        isTankDrive = true;
        activeList = tankDriveList;
    }

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
}
