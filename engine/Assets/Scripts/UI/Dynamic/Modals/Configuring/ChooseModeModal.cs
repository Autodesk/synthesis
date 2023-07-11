using System;
using Modes.MatchMode;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseModeModal : ModalDynamic {
    readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0);
        return u;
    };

    public ChooseModeModal() : base(new Vector2(300, 400)) {}

    public override void Create() {
        Title.SetText("Choose Mode");
        Description.SetText("Choose a mode to play in.");

        AcceptButton.RootGameObject.SetActive(false);
        CancelButton.Label.SetText("Close");

        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Practice Mode"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b => {
                if (SceneManager.GetActiveScene().name != "MainScene")
                    SceneManager.LoadScene("MainScene");
                ModeManager.CurrentMode = new PracticeMode();
            });

        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Match Mode"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b => {
                if (SceneManager.GetActiveScene().name != "MainScene")
                    SceneManager.LoadScene("MainScene");
                ModeManager.CurrentMode = new MatchMode();
            });

        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Server Test Mode"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b => {
                if (SceneManager.GetActiveScene().name != "MainScene")
                    SceneManager.LoadScene("MainScene");
                ModeManager.CurrentMode = new ServerTestMode();
            });

        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Open Empty Test Server"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b => {
                if (SceneManager.GetActiveScene().name != "MainScene")
                    SceneManager.LoadScene("MainScene");
                ModeManager.CurrentMode = new EmptyServerTestMode();
            });

        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Connect to Existing Server Test"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b => {
                if (SceneManager.GetActiveScene().name != "MainScene")
                    SceneManager.LoadScene("MainScene");
                ModeManager.CurrentMode = new ConnectToServerTestMode();
            });
    }

    public override void Update() {}

    public override void Delete() {}
}