using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Synthesis.Util;
using SynthesisAPI.Simulation;
using UnityEngine;

public class SimulationManagerInspector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }
    SimObject obj;

    // Update is called once per frame
    void Update()
    {
        if (obj == null)
        {
            try
            {
                obj = SimulationManager.SimulationObjects["TestDriveTrain v19"];
            }
            catch
            {
                 // ignored
            }
        }
        Debug.Log(obj.State);
    }
}
