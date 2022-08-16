using System;
using System.Collections;
using System.Collections.Generic;
using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using Synthesis.Util;
using UnityEngine;

using Logger = SynthesisAPI.Utilities.Logger;
using Synthesis.UI;
using UnityEngine.SceneManagement;
using Synthesis.Physics;

namespace Synthesis.Runtime {
    public class SimulationRunner : MonoBehaviour {

        private static uint _simulationContext = 0x00000001;
        public static uint SimulationContext => _simulationContext;

        public const uint RUNNING_SIM_CONTEXT = 0x00000001;
        public const uint PAUSED_SIM_CONTEXT =  0x00000002;
        public const uint REPLAY_SIM_CONTEXT =  0x00000004;
        public const uint GIZMO_SIM_CONTEXT =   0x00000008;

        public static event Action OnUpdate;
        private static bool _inSim = false;
        public static bool InSim {
            get => _inSim;
            set {
                _inSim = value;
                if (!_inSim)
                    SimKill();
            }
        }

        private bool _setupSceneSwitchEvent = false;

        void Start() {

            InSim = true;

            if (!_setupSceneSwitchEvent) {
                SceneManager.sceneUnloaded += (Scene s) => {
                    if (s.name == "MainScene") {
                        
                    }
                    // SimulationManager.SimulationObjects.ForEach(x => {
                    //     SimulationManager.RemoveSimObject(x.Value);
                    // });
                };
                _setupSceneSwitchEvent = true;
            }

            SetContext(RUNNING_SIM_CONTEXT);
            Synthesis.PreferenceManager.PreferenceManager.Load();
            MainHUD.Setup();
            ModeManager.Start();
            RobotSimObject.Setup();

            OnUpdate += DynamicUIManager.Update;

            // Screen.fullScreenMode = FullScreenMode.MaximizedWindow;

            // TestColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE));
            // RotationalDriver.TestSphericalCoordinate();

            if (ColorManager.HasColor("tree")) {
                GameObject.Instantiate(Resources.Load("Misc/Tree"));
            }
        }

        private void TestColor(Color c) {
            Debug.Log($"{c.r * 255}, {c.g * 255}, {c.b * 255}, {c.a * 255}");
            var hex = c.ToHex();
            Debug.Log(hex);
            var color = hex.ColorToHex();
            Debug.Log($"{color.r * 255}, {color.g * 255}, {color.b * 255}, {color.a * 255}");
        }

        void Update() {
            InputManager.UpdateInputs(_simulationContext);
            SimulationManager.Update();
            ModeManager.Update();

            // Debug.Log($"WHAT: {Time.realtimeSinceStartup}");

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

        public static void SimKill() {
            FieldSimObject.DeleteField();
            if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
                SimulationManager.RemoveSimObject(RobotSimObject.GetCurrentlyPossessedRobot());

            PhysicsManager.Reset();
        }
    }
}
