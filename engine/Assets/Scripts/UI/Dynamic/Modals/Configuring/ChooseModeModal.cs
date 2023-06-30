using System;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseModeModal : ModalDynamic
{
     readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0);
        return u;
    };
    
    public ChooseModeModal() : base(new Vector2(300, 170)) {}

    public override void Create()
    {
        Title.SetText("Choose Mode");
        Description.SetText("Choose a mode to play in.");
        
        AcceptButton.RootGameObject.SetActive(false);
        CancelButton.Label.SetText("Close");

        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Practice Mode"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b =>
            {
                if (SceneManager.GetActiveScene().name != "MainScene") SceneManager.LoadScene("MainScene");
                ModeManager.CurrentMode = new PracticeMode();
            });

        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Match Mode"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b =>
            {
                if (SceneManager.GetActiveScene().name != "MainScene") SceneManager.LoadScene("MainScene");
                ModeManager.CurrentMode = new MatchMode();
            });

        MainContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Server Test Mode"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b =>
            {
                ModeManager.CurrentMode = new ServerTestMode();
                SceneManager.LoadScene("MainScene");
            });
    }
    
    public override void Update() {}

    public override void Delete()
    {
    }
}