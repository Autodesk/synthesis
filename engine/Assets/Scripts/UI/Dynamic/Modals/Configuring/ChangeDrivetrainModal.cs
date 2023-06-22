using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class ChangeDrivetrainModal : ModalDynamic {

    public const float MODAL_WIDTH  = 400f;
    public const float MODAL_HEIGHT = 100f;

    private RobotSimObject.DrivetrainType _selectedType;

    public ChangeDrivetrainModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {
    }
    public override void Create() {
        Title.SetText("Change Drivetrain");

        AcceptButton.AddOnClickedEvent(b => {
            RobotSimObject.GetCurrentlyPossessedRobot().ConfiguredDrivetrainType = _selectedType;
            DynamicUIManager.CloseActiveModal();
        });

        _selectedType = RobotSimObject.GetCurrentlyPossessedRobot().ConfiguredDrivetrainType;

        MainContent.CreateLabeledDropdown()
            .SetTopStretch<LabeledDropdown>()
            .StepIntoLabel(l => l.SetText("Type"))
            .StepIntoDropdown(
                d => d.SetOptions(RobotSimObject.DRIVETRAIN_TYPES.Select(x => x.Name).ToArray())
                         .AddOnValueChangedEvent((d, i, o) => _selectedType = RobotSimObject.DRIVETRAIN_TYPES[i])
                         .SetValue(_selectedType.Value));
    }
    public override void Update() {
    }
    public override void Delete() {
    }
}
