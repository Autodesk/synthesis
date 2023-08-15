using System;
using Synthesis.UI.Dynamic;
using UI.Dynamic.Modals.Spawning;
using UnityEngine;

public class ChooseRobotTypeModal : ModalDynamic {
    public ChooseRobotTypeModal() : base(new Vector2(400, 65)) {}

    public override void Create() {
        Title.SetText("Choose Robot Type");

        AcceptButton.RootGameObject.SetActive(false);

        CancelButton.StepIntoLabel(l => l.SetText("Back")).AddOnClickedEvent(
            _ => DynamicUIManager.CreateModal<SpawningModal>());

        var spacing           = 22f;
        (var left, var right) = MainContent.SplitLeftRight((MainContent.Size.x / 2f) - (spacing), spacing * 2);
        var normal            = left.CreateButton("Regular")
                         .ApplyTemplate(UIComponent.VerticalLayout)
                         .AddOnClickedEvent(b => DynamicUIManager.CreateModal<AddRobotModal>())
                         .StepIntoLabel(l => l.SetText("Regular"));

        var MixAndMatch = right.CreateButton("Custom")
                              .ApplyTemplate(UIComponent.VerticalLayout)
                              .AddOnClickedEvent(b => DynamicUIManager.CreateModal<SpawnMixAndMatchModal>())
                              .StepIntoLabel(l => l.SetText("Custom"));
    }

    public override void Delete() {}

    public override void Update() {}
}