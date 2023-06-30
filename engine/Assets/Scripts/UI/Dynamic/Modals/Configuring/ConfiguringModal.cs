using System;
using System.Collections;
using System.Collections.Generic;
using Synthesis.Gizmo;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class ConfiguringModal : ModalDynamic {
    public ConfiguringModal()
        : base(new Vector2(400, 30 + 7.5f + 50 + 7.5f + 30 + 7.5f + 50 + 15 /*To space better*/)) {}

    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset);
        return u;
    };

    public override void Create() {
        Title.SetText("Configuring");
        Description.SetText("What do you want to change?");
        ModalImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("wrench-icon"));
        ModalImage.SetColor(ColorManager.SYNTHESIS_WHITE);

        var gpmLabel   = MainContent.CreateLabel(height: 30f).SetTopStretch<Label>().SetText("Gamepiece Manipulation");
        var subContent = MainContent.CreateSubContent(new Vector2(MainContent.Size.x, 50))
                             .SetTopStretch<Content>(anchoredY: gpmLabel.Size.y + 7.5f);
        var spacing = 15f;
        {
            (var left, var right) = subContent.SplitLeftRight((MainContent.Size.x / 2f) - (spacing / 2f), spacing);
            var robot             = left.CreateButton()
                            .SetTopStretch<Button>()
                            .AddOnClickedEvent(b => {
                                DynamicUIManager.CloseActiveModal();
                                DynamicUIManager.CreatePanel<ConfigureGamepiecePickupPanel>();
                            })
                            .StepIntoLabel(l => l.SetText("Pickup"));
            var field = right.CreateButton()
                            .SetTopStretch<Button>()
                            .AddOnClickedEvent(b => {
                                DynamicUIManager.CloseActiveModal();
                                DynamicUIManager.CreatePanel<ConfigureShotTrajectoryPanel>();
                            })
                            .StepIntoLabel(l => l.SetText("Ejector"));
        }
        var robotLabel = MainContent.CreateLabel(height: 30f).ApplyTemplate(VerticalLayout).SetText("General Robot");
        var subContent2 =
            MainContent.CreateSubContent(new Vector2(MainContent.Size.x, 50)).ApplyTemplate(VerticalLayout);
        // Using scopes because I can't be bothered to rename -Hunter
        {
            (var left, var right) = subContent2.SplitLeftRight((MainContent.Size.x / 2f) - (spacing / 2f), spacing);
            var move              = left.CreateButton("Move Robot").SetTopStretch<Button>().AddOnClickedEvent(b => {
                DynamicUIManager.CloseActiveModal();
                GizmoManager.SpawnGizmo(RobotSimObject.GetCurrentlyPossessedRobot());
                         });
            // var field = right.CreateButton("Field")
            //     .ApplyTemplate<Button>(VerticalLayout)
            //     .AddOnClickedEvent(b => DynamicUIManager.CreateModal<AddFieldModal>())
            //     .StepIntoLabel(l => l.SetText("Fields"));
        }
    }

    public override void Delete() {}

    public override void Update() {}
}
