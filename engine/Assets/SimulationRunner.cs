using System;
using System.Collections;
using System.Collections.Generic;
using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

using Logger = SynthesisAPI.Utilities.Logger;

public class SimulationRunner : MonoBehaviour {

    public static event Action OnUpdate;

    void Start() {
        PreferenceManager.Load();

        OnUpdate += DynamicUIManager.Update;

        // ModalManager.CreateModal<AddRobotModal>();
    }

    void Update() {
        SimulationManager.Update();

        if (OnUpdate != null)
            OnUpdate();

        // if (Input.GetKeyDown(KeyCode.K)) {
        //     if (!SimulationManager.RemoveSimObject(RobotSimObject.CurrentlyPossessedRobot))
        //         Logger.Log("Failed", LogLevel.Debug);
        //     else
        //         Logger.Log("Succeeded", LogLevel.Debug);
        // }
    }

    void OnDestroy() {
        PreferenceManager.Save();
    }
}
