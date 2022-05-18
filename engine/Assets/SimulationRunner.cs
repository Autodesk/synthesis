using System.Collections;
using System.Collections.Generic;
using Synthesis.PreferenceManager;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

using Logger = SynthesisAPI.Utilities.Logger;

public class SimulationRunner : MonoBehaviour {
    void Start() {
        PreferenceManager.Load();
    }

    void Update() {
        SimulationManager.Update();

        if (Input.GetKeyDown(KeyCode.K)) {
            if (!SimulationManager.RemoveSimObject(RobotSimObject.CurrentlyPossessedRobot))
                Logger.Log("Failed", LogLevel.Debug);
            else
                Logger.Log("Succeeded", LogLevel.Debug);
        }
    }

    void OnDestroy() {
        PreferenceManager.Save();
    }
}
