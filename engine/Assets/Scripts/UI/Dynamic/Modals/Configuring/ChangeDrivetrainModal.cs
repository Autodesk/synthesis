using System;
using System.Collections;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class ChangeDrivetrainModal : ModalDynamic {

    public const float MODAL_WIDTH = 100f;
    public const float MODAL_HEIGHT = 100f;

    private RobotSimObject.DrivetrainType _selectedType;

    public ChangeDrivetrainModal(): base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) { }
    public override void Create() {
        Title.SetText("Change Drivetrain");

        AcceptButton.AddOnClickedEvent(b => RobotSimObject.GetCurrentlyPossessedRobot().ConfiguredDrivetrainType = _selectedType);

        _selectedType = RobotSimObject.GetCurrentlyPossessedRobot().ConfiguredDrivetrainType;

        MainContent.CreateLabeledDropdown().SetTopStretch<LabeledDropdown>().StepIntoLabel(l => l.SetText("Type"))
            .StepIntoDropdown(d => d.SetOptions(Enum.GetNames(typeof(RobotSimObject.DrivetrainType)))
                .AddOnValueChangedEvent(
                    (d, i, o) => _selectedType = RobotSimObject.DRIVETRAIN_TYPES[i])
                .SetValue(_selectedType.Value));
    }
    public override void Update() { }
    public override void Delete() { }
}
