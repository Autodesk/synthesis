using Synthesis.UI.Dynamic;
using UnityEngine;

public class ConfigMotorModal : ModalDynamic {
    public ConfigMotorModal(): base(new Vector2(500, 480)) {}

    public override void Create() {
        Title.SetText("Motor Con");
        Description.SetText("Change motor settings");

        AcceptButton.StepIntoLabel(b => { b.SetText("Save"); }).AddOnClickedEvent(b => {
            RobotSimObject.GetCurrentlyPossessedRobot().MiraLive.Save();
            DynamicUIManager.CloseActiveModal();
        });

        
    }

    public override void Update() { }
    public override void Delete() { }

}