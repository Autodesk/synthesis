using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Meant to be used for selecting a robot in the main menu
/// </summary>
public class SelectFieldScrollable : SelectScrollable
{
    /// <summary>
    /// Initializes a new <see cref="SelectFieldScrollable"/> instance.
    /// </summary>
    public SelectFieldScrollable() : base("definition.bxdf", "No fields found in directory!")
    {
    }
}
