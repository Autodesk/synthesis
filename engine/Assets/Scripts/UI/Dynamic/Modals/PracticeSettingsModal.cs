using System;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class PracticeSettingsModal : ModalDynamic
{
    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 15f);
        return u;
    };
    
    public PracticeSettingsModal() : base(new Vector2(500, 220))
    {
        
    }

    public override void Create()
    {
        Title.SetText("Practice Settings");
        Description.SetText("Configuration actions for practice mode");
        
        // TODO fix vertical spacing

        MainContent.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .StepIntoLabel(label => label.SetText("Reset Robot"))
            .AddOnClickedEvent(b => ModeManager.ResetRobot());

        MainContent.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .StepIntoLabel(label => label.SetText("Reset Gamepieces"))
            .AddOnClickedEvent(b => ModeManager.ResetGamepieces());

        MainContent.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .StepIntoLabel(label => label.SetText("Reset Field"))
            .AddOnClickedEvent(b => ModeManager.ResetField());

        MainContent.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .StepIntoLabel(label => label.SetText("Reset All"))
            .AddOnClickedEvent(b => ModeManager.ResetAll());
    }
    
    public override void Update(){}
    
    public override void Delete(){}
}