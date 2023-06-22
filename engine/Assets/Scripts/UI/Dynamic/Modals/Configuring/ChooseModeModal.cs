using System;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseModeModal : ModalDynamic {
    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0);
        return u;
    };

    public ChooseModeModal() : base(new Vector2(300, 120)) {
    }

    public override void Create() {
        Title.SetText("Choose Mode");
        Description.SetText("Choose a mode to play in.");

        AcceptButton.RootGameObject.SetActive(false);
        CancelButton.Label.SetText("Close");

        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Practice Mode"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b => {
                ModeManager.CurrentMode = new PracticeMode();
                SceneManager.LoadScene("MainScene");
            });

        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Coming Soon").SetColor(ColorManager.SYNTHESIS_WHITE))
            .StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK_ACCENT))
            .ApplyTemplate(VerticalLayout);

        // MainContent.CreateButton()
        //     .StepIntoLabel(l => l.SetText("Match Mode"))
        //     .ApplyTemplate(VerticalLayout)
        //     .AddOnClickedEvent(b =>
        //     {
        //         ModeManager.CurrentMode = new MatchMode();
        //         SceneManager.LoadScene("MainScene");
        //     });
    }

    public override void Update() {
    }

    public override void Delete() {
    }
}