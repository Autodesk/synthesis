using System;
using System.Collections;
using System.Collections.Generic;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UI.Dynamic.Modals.Spawning;
using UnityEngine;
using Utilities.ColorManager;

public class SpawningModal : ModalDynamic {
    public SpawningModal() : base(new Vector2(400, 65)) {}

    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset);
        return u;
    };

    public override void Create() {
        Title.SetText("Spawning");

        ModalIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("PlusIcon"))
            .SetColor(ColorManager.SynthesisColor.MainText);

        AcceptButton.RootGameObject.SetActive(false);

        var spacing           = 22f;
        (var left, var right) = MainContent.SplitLeftRight((MainContent.Size.x / 2f) - (spacing), spacing * 2);
        var robot             = left.CreateButton("Robot")
                        .ApplyTemplate<Button>(VerticalLayout)
                        .AddOnClickedEvent(b => DynamicUIManager.CreateModal<AddRobotModal>())
                        .StepIntoLabel(l => l.SetText("Robots"));

        var field = right.CreateButton("Field")
                        .ApplyTemplate<Button>(VerticalLayout)
                        .AddOnClickedEvent(b => DynamicUIManager.CreateModal<AddFieldModal>())
                        .StepIntoLabel(l => l.SetText("Fields"));
    }

    public override void Delete() {}

    public override void Update() {}
}
