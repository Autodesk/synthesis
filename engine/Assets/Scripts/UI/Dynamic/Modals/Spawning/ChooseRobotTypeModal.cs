using Synthesis.UI.Dynamic;
using UI.Dynamic.Modals.Spawning;
using UnityEngine;

public class ChooseRobotTypeModal : ModalDynamic {
    public ChooseRobotTypeModal() : base(new Vector2(400, 65)) {}

    public override void Create() {
        Title.SetText("Choose Robot Type");

        ModalIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("plus"));

        AcceptButton.RootGameObject.SetActive(false);

        CancelButton.StepIntoLabel(l => l.SetText("Back"))
            .AddOnClickedEvent(
                _ => DynamicUIManager.CreateModal<SpawningModal>());

        var spacing       = 22f;
        var (left, right) = MainContent.SplitLeftRight((MainContent.Size.x / 2f) - (spacing), spacing * 2);
        var normal        = left.CreateButton("Regular")
                         .ApplyTemplate(UIComponent.VerticalLayout)
                         .AddOnClickedEvent(
                             _ => DynamicUIManager.CreateModal<AddRobotModal>())
                         .StepIntoLabel(l => l.SetText("Regular"));

        var Custom = right.CreateButton("Custom")
                         .ApplyTemplate(UIComponent.VerticalLayout)
                         .AddOnClickedEvent(
                             _ => DynamicUIManager.CreateModal<AddMixAndMatchModal>())
                         .StepIntoLabel(l => l.SetText("Custom"))
                         .StepIntoImage(i => i.InvertGradient());
    }

    public override void Delete() {}

    public override void Update() {}
}