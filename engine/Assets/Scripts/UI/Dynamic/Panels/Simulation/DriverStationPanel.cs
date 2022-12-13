using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.UI.Dynamic;
using Synthesis.UI;
using Synthesis.WS;
using SynthesisAPI.RoboRIO;

public class DriverStationPanel : PanelDynamic {

    private Button _modeButton;

    public DriverStationPanel() : base(new Vector2(800, 60)) {}

    public override void Create() {

        // BottomCenter the panel
        var transform = base.UnityObject.GetComponent<RectTransform>();
        transform.pivot = new Vector2(0.5f, 0.0f);
        transform.anchorMax = new Vector2(0.5f, 0.0f);
        transform.anchorMin = new Vector2(0.5f, 0.0f);
        transform.anchoredPosition = new Vector2(0.0f, 10.0f);

        Title.SetText("Driver Station (MOCKUP)");
        Title.SetWidth<Label>(400);

        _modeButton = MainContent.CreateButton();
        _modeButton.SetWidth<Button>(200f).SetLeftStretch<Button>(topPadding: 0f, bottomPadding: 0f);
        SetModeButton(false);
        _modeButton.AddOnClickedEvent(b => {
            if (!WebSocketManager.Initialized || !WebSocketManager.HasClient) {
                SetModeButton(false);
                return;
            }

            var currentData = WebSocketManager.RioState.GetData<DriverStationData>("DriverStation", "");
            if (currentData.Enabled) {
                WebSocketManager.UpdateData<DriverStationData>("DriverStation", "", d => {
                    d.Enabled = false;
                });
                SetModeButton(false);
            } else {
                WebSocketManager.UpdateData<DriverStationData>("DriverStation", "", d => {
                    d.Enabled = true;
                });
                SetModeButton(true);
            }
        });

        RobotSimObject.GetCurrentlyPossessedRobot().UseSimulationBehaviour = true;
    }

    private void SetModeButton(bool isEnabled) {
        if (isEnabled) {
            _modeButton.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK_ACCENT))
                .StepIntoLabel(l => l.SetColor(ColorManager.SYNTHESIS_ORANGE)
                    .SetText("Enabled"));
        } else {
            _modeButton.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE))
                .StepIntoLabel(l => l.SetColor(ColorManager.SYNTHESIS_BLACK)
                    .SetText("Disabled"));
        }
    }

    public override void Delete() {
        RobotSimObject.GetCurrentlyPossessedRobot().UseSimulationBehaviour = false;
    }

    public override void Update() { }
}
