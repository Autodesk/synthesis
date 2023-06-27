using Synthesis.UI.Dynamic;
using UnityEngine;

public class ConfigMotorModal : ModalDynamic {
    public ConfigMotorModal(): base(new Vector2(500, 480)) {}

    public override void Create() {
        Title.SetText("Motor C");
        Description.SetText("Change motor settings");


    }

    public override void Update() { }
    public override void Delete() { }

}