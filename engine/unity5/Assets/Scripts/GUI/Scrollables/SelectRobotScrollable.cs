using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Meant to be used for selecting a robot in the main menu
/// </summary>
public class SelectRobotScrollable : SelectScrollable
{
    /// <summary>
    /// Initializes a new <see cref="SelectRobotScrollable"/> instance.
    /// </summary>
    public SelectRobotScrollable() : base("skeleton.bxdj", "No robots found in directory!")
    {
    }
}
