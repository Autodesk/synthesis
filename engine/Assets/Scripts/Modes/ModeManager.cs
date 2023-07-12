using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeManager {
    private static bool _modeHasStarted  = false;
    public static bool ModeHasStarted   => _modeHasStarted;

    private static IMode _currentMode;
    public static IMode CurrentMode {
        get => _currentMode;
        set {
            if (_currentMode != null)
                _currentMode.End();
            _currentMode    = value;
            _modeHasStarted = false;

            // this is always called in GridMenuScene so _currentMode is never started here
            // it is now started in SimulationRunner::Start
            if (SceneManager.GetActiveScene().name == "MainScene" && _currentMode != null) {
                _currentMode.Start();
                _modeHasStarted = true;
            }
        }
    }

    public static void Start() {
        if (CurrentMode == null) {
            DynamicUIManager.CreateModal<ChooseModeModal>();
        } else if (!ModeHasStarted) {
            CurrentMode.Start();
        }
    }

    public static void Update() {
        CurrentMode?.Update();
    }

    public static void ModalClosed() {
        if (CurrentMode != null)
            CurrentMode.CloseMenu();
    }

    public static void Teardown() {
        CurrentMode = null;
    }
}