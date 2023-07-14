using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Synthesis.UI.Dynamic;
using UnityEngine;
using SynthesisAPI.Utilities;

using Logger = SynthesisAPI.Utilities.Logger;

public class ChangeDrivetrainModal : ModalDynamic {
    public const float MODAL_WIDTH  = 400f;
    public const float MODAL_HEIGHT = 100f;

    private RobotSimObject.DrivetrainType _selectedType;

    public ChangeDrivetrainModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

    public override void Create() {
        if (RobotSimObject.CurrentlyPossessedRobot == string.Empty) {
            return;
        }

        Title.SetText("Change Drivetrain");
        Description.SetText("Select the drivetrain you want to use");
        AcceptButton.AddOnClickedEvent(b => {
            MainHUD.ConfigRobot.ConfiguredDrivetrainType = _selectedType;
            DynamicUIManager.CloseActiveModal();
        });

        _selectedType = MainHUD.ConfigRobot.ConfiguredDrivetrainType;

        MainContent.CreateLabeledDropdown()
            .SetTopStretch<LabeledDropdown>()
            .StepIntoLabel(l => l.SetText("Type"))
            .StepIntoDropdown(
                d => d.SetOptions(RobotSimObject.DRIVETRAIN_TYPES.Select(x => x.Name).ToArray())
                         .AddOnValueChangedEvent((d, i, o) => _selectedType = RobotSimObject.DRIVETRAIN_TYPES[i])
                         .SetValue(_selectedType.Value));
    }

    public override void Update() {
        if (RobotSimObject.CurrentlyPossessedRobot == string.Empty) {
            Logger.Log("Must spawn a robot first", LogLevel.Info);
            DynamicUIManager.CloseActiveModal();
        }
    }

    public override void Delete() {}
}
