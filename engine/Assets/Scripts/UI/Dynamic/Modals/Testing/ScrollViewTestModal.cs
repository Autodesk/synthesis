using System.Collections;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class ScrollViewTestModal : ModalDynamic {
    public ScrollViewTestModal() : base(new Vector2(1200, 700)) {}

    public override void Create() {
        Title.SetText("Testing Scrollview");

        var sv = MainContent.CreateScrollView().SetStretch<ScrollView>();
        CreateItem(sv, Color.red);
        CreateItem(sv, Color.black);
        CreateItem(sv, Color.green);
        sv.Content.SetTopStretch<Content>().SetHeight<Content>(-sv.Content.RectOfChildren().yMin);
    }

    public void CreateItem(ScrollView sv, Color c) {
        var content = sv.Content.CreateSubContent(new Vector2(sv.Content.Size.x, 400));
        content.EnsureImage().StepIntoImage(i => i.SetColor(c));
        content.SetTopStretch<ScrollView>(anchoredY: -sv.Content.RectOfChildren(content).yMin);
    }

    public override void Delete() {}

    public override void Update() {}
}
