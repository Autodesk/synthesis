using System;
using System.Collections;
using System.Collections.Generic;
using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

using Logger = SynthesisAPI.Utilities.Logger;

namespace Synthesis.Runtime {
    public class SimulationRunner : MonoBehaviour {

        private static uint _simulationContext = 0x00000001;
        public static uint SimulationContext => _simulationContext;

        public const uint RUNNING_SIM_CONTEXT = 0x00000001;
        public const uint PAUSED_SIM_CONTEXT = 0x00000002;
        public const uint REPLAY_SIM_CONTEXT = 0x00000004;

        public static event Action OnUpdate;

        void Start() {
            SetContext(RUNNING_SIM_CONTEXT);
            Synthesis.PreferenceManager.PreferenceManager.Load();

            OnUpdate += DynamicUIManager.Update;
        }

        void Update() {
            InputManager.UpdateInputs(_simulationContext);
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
            Synthesis.PreferenceManager.PreferenceManager.Save();
        }

        public static void SetContext(uint c) {
            _simulationContext = c;
        }
        public static void AddContext(uint c) {
            _simulationContext |= c;
        }
        public static void RemoveContext(uint c) {
            if (HasContext(c))
                _simulationContext ^= c;
        }
        public static bool HasContext(uint c)
            => (_simulationContext & c) != 0;
    }
}
