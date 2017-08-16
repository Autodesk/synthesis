using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIField : MonoBehaviour
{
    public void BeginEdit()
    {
        Robot.ControlsEnabled = false;
    }

    public void EndEdit()
    {
        Robot.ControlsEnabled = true;
    }
}
