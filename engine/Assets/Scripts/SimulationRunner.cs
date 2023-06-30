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
using SynthesisAPI.EventBus;
using Synthesis.Replay;
using Synthesis.WS;
using SynthesisAPI.RoboRIO;
using UnityEngine.Rendering;

namespace Synthesis.Runtime {
    public class SimulationRunner : MonoBehaviour {

        private static uint _simulationContext = 0x00000001;
        public static uint SimulationContext => _simulationContext;

        public const uint RUNNING_SIM_CONTEXT = 0x00000001;
        public const uint PAUSED_SIM_CONTEXT =  0x00000002;
        public const uint REPLAY_SIM_CONTEXT =  0x00000004;
        public const uint GIZMO_SIM_CONTEXT =   0x00000008;

        /// <summary>
        /// Called when going to the main menu.
        /// Will be completely reset after called
        /// </summary>
        public static event Action OnSimKill;

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
            WebSocketManager.Init();

            OnUpdate += DynamicUIManager.Update;

            WebSocketManager.RioState.OnUnrecognizedMessage += s => Debug.Log(s);

            // Screen.fullScreenMode = FullScreenMode.MaximizedWindow;

            // TestColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE));
            // RotationalDriver.TestSphericalCoordinate();

            if (ColorManager.HasColor("tree")) {
                GameObject.Instantiate(Resources.Load("Misc/Tree"));
            }

            SettingsModal.LoadSettings();
            SettingsModal.ApplySettings();
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

            // Debug.Log($"WHAT: {Time.realtimeSinceStartup}");

            if (OnUpdate != null)
                OnUpdate();

            // var socket = WebSocketManager.RioState.GetData<PWMData>("PWM", "0");
            // if (socket.GetData() == null) {
            //     Debug.Log("Data null");
            // }
            // Debug.Log($"{socket.Init}:{socket.Speed}:{socket.Position}");

            // var aiData = WebSocketManager.RioState.GetData<AIData>("AI", "3");
            // if (aiData.Init) {
            //     WebSocketManager.UpdateData<AIData>("AI", "3", d => {
            //         d.Voltage = 2.3;
            //     });
            // }

            // if (Input.GetKeyDown(KeyCode.K)) {
            //     if (!SimulationManager.RemoveSimObject(RobotSimObject.CurrentlyPossessedRobot))
            //         Logger.Log("Failed", LogLevel.Debug);
            //     else
            //         Logger.Log("Succeeded", LogLevel.Debug);
            // }
        }

        private void FixedUpdate() {
            SimulationManager.FixedUpdate();
            PhysicsManager.FixedUpdate();
        }

        void OnDestroy() {
            Synthesis.PreferenceManager.PreferenceManager.Save();
        }

        /// <summary>
        /// Set current context
        /// </summary>
        /// <param name="c">Mask for context</param>
        public static void SetContext(uint c) {
            _simulationContext = c;
        }
        /// <summary>
        /// Add an additional context to the current contexts
        /// </summary>
        /// <param name="c">Mask for context</param>
        public static void AddContext(uint c) {
            _simulationContext |= c;
        }
        /// <summary>
        /// Remove a context from the current context
        /// </summary>
        /// <param name="c">Mask for context</param>
        public static void RemoveContext(uint c) {
            if (HasContext(c))
                _simulationContext ^= c;
        }
        /// <summary>
        /// Check if a context exists within the current context
        /// </summary>
        /// <param name="c">Mask for context</param>
        /// <returns></returns>
        public static bool HasContext(uint c)
            => (_simulationContext & c) != 0;

        /// <summary>
        /// Teardown sim for recycle
        /// </summary>
        public static void SimKill() {
            FieldSimObject.DeleteField();
            List<string> robotIDs = new List<string>(RobotSimObject.SpawnedRobots.Count);
            RobotSimObject.SpawnedRobots.ForEach(x => robotIDs.Add(x.Name));
            robotIDs.ForEach(x => RobotSimObject.RemoveRobot(x));
            OrbitCameraMode.FocusPoint = () => Vector3.zero;

            if (OnSimKill != null)
                OnSimKill();

            OnSimKill = null;

            PhysicsManager.Reset();
            ReplayManager.Teardown();
            WebSocketManager.Teardown();
        }
    }
}
