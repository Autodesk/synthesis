using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.Simulation;
using UnityEngine;

public class SimulationRunner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SimulationManager.Update();
    }
}
