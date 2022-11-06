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

        for (int i = 0; i < 10; i++) {
            CreateItem("PWM " + i);
        }

        CreateAddButton();

        _scrollView.Content.SetTopStretch<Content>().SetHeight<Content>(-_scrollView.Content.RectOfChildren().yMin);
    }

    public void CreateItem(string text) {
        var content = _scrollView.Content.CreateSubContent(new Vector2(_scrollView.Content.Size.x, 80));
        content.EnsureImage().StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE));
        content.SetTopStretch<Content>(anchoredY: -_scrollView.Content.RectOfChildren(content).yMin);
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
        content.Image?.SetColor(new Color(0, 0, 0, 0));
        content.SetTopStretch<Content>(anchoredY: -_scrollView.Content.RectOfChildren(content).yMin);
        var button = content.CreateButton("Add");
        button.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE));
        button.RootRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        button.RootRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        button.RootRectTransform.anchoredPosition = Vector2.zero;
        button.SetPivot<Button>(new Vector2(0.5f, 0.5f));
        button.SetSize<Button>(new Vector2(150, 50));
        button.AddOnClickedEvent(b => {
            DynamicUIManager.CreateModal<RCIOConfigModal>();
        });
    }

    public override void Delete() { }

    public override void Update() { }
}

public class RCIOConfigModal : ModalDynamic {
    public RCIOConfigModal() : base(new Vector2(300, 200)) { }

    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset);
        return u;
    };

    public override void Create() {
        Title.SetText("Create IO");
        Title.SetWidth<Label>(300);
        Description.SetText("Make a RoboRIO IO");
        ModalImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("wrench-icon"));
        ModalImage.SetColor(ColorManager.SYNTHESIS_WHITE);

        var dropdown = MainContent.CreateLabeledDropdown().StepIntoLabel(l => l.SetText("Type"));
        dropdown.StepIntoDropdown(d => d.SetOptions(new string[] { "PWM", "Analog", "Digital" }));
    }

    public override void Delete() { }

    public override void Update() { }
}
