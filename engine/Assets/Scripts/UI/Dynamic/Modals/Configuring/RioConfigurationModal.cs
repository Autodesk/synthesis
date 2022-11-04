using System;
using System.Collections;
using System.Collections.Generic;
using Synthesis.Gizmo;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class RioConfigurationModal : ModalDynamic {
    public RioConfigurationModal() : base(new Vector2(1000, 600)) { }

    private ScrollView _scrollView;

    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset);
        return u;
    };

    public override void Create() {
        Title.SetText("RoboRIO Configuration");
        Title.SetWidth<Label>(300);
        Description.SetText("Configuring RoboRIO for Synthesis simulation");
        ModalImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("wrench-icon"));
        ModalImage.SetColor(ColorManager.SYNTHESIS_WHITE);

        _scrollView = MainContent.CreateScrollView();
        _scrollView.SetStretch<ScrollView>();

        CreateItem("WooOOOooo");

        CreateAddButton();

        _scrollView.Content.SetTopStretch<Content>().SetHeight<Content>(-_scrollView.Content.RectOfChildren().yMin);
    }

    public void CreateItem(string text) {
        var content = _scrollView.Content.CreateSubContent(new Vector2(_scrollView.Content.Size.x, 80));
        content.EnsureImage().StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE));
        content.SetTopStretch<ScrollView>(anchoredY: -_scrollView.Content.RectOfChildren(content).yMin);
        content.CreateLabel().SetStretch<Label>(leftPadding: 20, topPadding: 20, bottomPadding:20)
            .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle).SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left)
            .SetText(text).SetColor(ColorManager.SYNTHESIS_BLACK);
        var button = content.CreateButton("Configure");
        button.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK));
        button.SetPivot<Button>(new Vector2(1, 0.5f)).SetRightStretch<Button>(20, 20, 15).SetWidth<Button>(150).SetHeight<Button>(-30);
        button.StepIntoLabel(l => l.SetColor(ColorManager.SYNTHESIS_WHITE));
    }

    public void CreateAddButton() {
        var content = _scrollView.Content.CreateSubContent(new Vector2(_scrollView.Content.Size.x, 80));
        content.EnsureImage().StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE));
        content.SetTopStretch<ScrollView>(anchoredY: -_scrollView.Content.RectOfChildren(content).yMin)
            .SetPivot<ScrollView>(new Vector2(0.5f, 0.5f)).SetWidth<ScrollView>(100).SetHeight<ScrollView>(60);
        content.CreateLabel().SetStretch<Label>(leftPadding: 20, topPadding: 20, bottomPadding:20)
            .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle).SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
            .SetText("Add").SetColor(ColorManager.SYNTHESIS_BLACK);
    }

    public override void Delete() { }

    public override void Update() { }
}
