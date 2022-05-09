using System.Collections;
using System.Collections.Generic;
using Synthesis.PreferenceManager;
using SynthesisAPI.Simulation;
using UnityEngine;

public class SimulationRunner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        PreferenceManager.Load();
    }

    // Update is called once per frame
    void Update()
    {
        SimulationManager.Update();
    }

    void OnDestroy() {
        PreferenceManager.Save();
    }
}
