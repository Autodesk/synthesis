using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.UI.Dynamic;
using Synthesis.UI;
using Synthesis.WS;
using SynthesisAPI.RoboRIO;
using SynthesisAPI.Utilities;
using Utilities.ColorManager;
using Logger = SynthesisAPI.Utilities.Logger;

public class DriverStationPanel : PanelDynamic {
    private const string DRIVERSTATION_CONNECTED     = "Driver Station (Connected)";
    private const string DRIVERSTATION_NOT_CONNECTED = "Driver Station (Not Connected)";

    private Button _modeButton;
    private Dropdown _modeSelection;

    private bool _useAuto = false;

    public DriverStationPanel() : base(new Vector2(800, 60)) {}

    public override bool Create() {
        TweenFromBottom = true;
        
        if (RobotSimObject.CurrentlyPossessedRobot == string.Empty) {
            Logger.Log("Spawn a robot first", LogLevel.Info);
            return false;
        }

        // BottomCenter the panel
        var transform              = base.UnityObject.GetComponent<RectTransform>();
        transform.pivot            = new Vector2(0.5f, 0.0f);
        transform.anchorMax        = new Vector2(0.5f, 0.0f);
        transform.anchorMin        = new Vector2(0.5f, 0.0f);
        transform.anchoredPosition = new Vector2(0.0f, 10.0f);

        Title.SetText("Driver Station (Not Connected)");

        AcceptButton.RootGameObject.SetActive(false);

        _modeButton = MainContent.CreateButton();
        _modeButton.SetWidth<Button>(200f).SetLeftStretch<Button>(topPadding: 0f, bottomPadding: 0f);
        SetModeButton(false);
        _modeButton.AddOnClickedEvent(b => {
            if (!WebSocketManager.Initialized || !WebSocketManager.HasClient) {
                SetModeButton(false);
                return;
            }

            var currentData = WebSocketManager.RioState.GetData<DriverStationData>("");
            if (currentData.Enabled) {
                WebSocketManager.UpdateData<DriverStationData>("", d => { d.Enabled = false; });
                SetModeButton(false);
            } else {
                WebSocketManager.UpdateData<DriverStationData>("", d => { d.Enabled = true; });
                SetModeButton(true);
            }
        });

        _modeSelection = MainContent.CreateDropdown();
        _modeSelection.SetWidth<Dropdown>(150f).SetLeftStretch<Button>(
            topPadding: 0f, bottomPadding: 20f, anchoredX: 220);
        _modeSelection.SetOptions(new string[] { "Teleop", "Auto" });
        _modeSelection.AddOnValueChangedEvent((d, i, o) => {
            _useAuto = i != 0; // Gonna have to change later
            WebSocketManager.UpdateData<DriverStationData>("", d => d.Autonomous = _useAuto);
        });

        RobotSimObject.GetCurrentlyPossessedRobot().UseSimulationBehaviour = true;

        return true;
    }

    private void SetModeButton(bool isEnabled) {
        if (isEnabled) {
            _modeButton.StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.InteractiveBackground))
                .StepIntoLabel(l => l.SetColor(ColorManager.SynthesisColor.MainText).SetText("Enabled"));
        } else {
            _modeButton.StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.InteractiveElementLeft,
                    ColorManager.SynthesisColor.InteractiveElementRight))
                .StepIntoLabel(l => l.SetColor(ColorManager.SynthesisColor.MainText).SetText("Disabled"));
        }
    }

    public override void Delete() {
        if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
            RobotSimObject.GetCurrentlyPossessedRobot().UseSimulationBehaviour = false;
    }

    public override void Update() {
        if (!WebSocketManager.Initialized || !WebSocketManager.HasClient) {
            Title.SetText(DRIVERSTATION_NOT_CONNECTED);
        } else {
            Title.SetText(DRIVERSTATION_CONNECTED);
        }
    }
}
