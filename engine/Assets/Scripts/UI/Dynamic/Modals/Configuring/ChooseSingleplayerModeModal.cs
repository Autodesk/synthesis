using System;
using Modes.MatchMode;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseSingleplayerModeModal : ModalDynamic {
    readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0);
        return u;
    };

    public ChooseSingleplayerModeModal() : base(new Vector2(230, 200)) {}

    public override void Create() {
        Title.SetText("Choose Mode");
        Description.SetText("Choose a mode to play in.");

        ModalIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("settings"));

        AcceptButton.RootGameObject.SetActive(false);
        CancelButton.RootGameObject.SetActive(false);

        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Practice Mode"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b => {
                ModeManager.CurrentMode = new PracticeMode();
                if (SceneManager.GetActiveScene().name != "MainScene")
                    SceneManager.LoadScene("MainScene");
            });

        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Match Mode"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b => {
                ModeManager.CurrentMode = new MatchMode();
                if (SceneManager.GetActiveScene().name != "MainScene")
                    SceneManager.LoadScene("MainScene");
            });
    }

    public override void Update() {}

    public override void Delete() {}
}