using System;
using Synthesis.UI.Dynamic;
using UI.Dynamic.Modals.Spawning;
using UnityEngine;
using Utilities.ColorManager;

public class SpawningModal : ModalDynamic {
    public SpawningModal() : base(new Vector2(400, 65)) {}

    public override void Create() {
        Title.SetText("Spawning");

        ModalIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("plus"))
            .SetColor(ColorManager.SynthesisColor.MainText);

        AcceptButton.RootGameObject.SetActive(false);

        var spacing           = 22f;
        (var left, var right) = MainContent.SplitLeftRight((MainContent.Size.x / 2f) - (spacing), spacing * 2);
        var robot             = left.CreateButton("Robot")
                        .ApplyTemplate(UIComponent.VerticalLayout)
                        .AddOnClickedEvent(b => DynamicUIManager.CreateModal<ChooseRobotTypeModal>())
                        .StepIntoLabel(l => l.SetText("Robots"));

        var field = right.CreateButton("Field")
                        .ApplyTemplate(UIComponent.VerticalLayout)
                        .AddOnClickedEvent(b => DynamicUIManager.CreateModal<AddFieldModal>())
                        .StepIntoLabel(l => l.SetText("Fields"))
                        .StepIntoImage(i => i.InvertGradient());
    }

    public override void Delete() {}

    public override void Update() {}
}
