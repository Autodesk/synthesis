using System;
using Modes.MatchMode;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseMultiplayerModeModal : ModalDynamic {
    readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0);
        return u;
    };

    public ChooseMultiplayerModeModal() : base(new Vector2(230, 300)) {}

    public override void Create() {
        Title.SetText("Choose Mode");
        Description.SetText("Choose a mode to play in.");

        ModalIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("settings"));

        AcceptButton.RootGameObject.SetActive(false);
        CancelButton.Label.SetText("Close");
        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Server Test Mode"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b => {
                ModeManager.CurrentMode = new ServerTestMode();
                if (SceneManager.GetActiveScene().name != "MainScene")
                    SceneManager.LoadScene("MainScene");
            });

        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Host a Server"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b => {
                if (SceneManager.GetActiveScene().name != "MainScene")
                    SceneManager.LoadScene("MainScene");
                ModeManager.CurrentMode = new ServerHostingMode();
            });

        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Connect to a sServer"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b => {
                if (SceneManager.GetActiveScene().name != "MainScene")
                    SceneManager.LoadScene("MainScene");
                ModeManager.CurrentMode = new ConnectToMultiplayerMode();
            });
    }

    public override void Update() {}

    public override void Delete() {}
}