using Synthesis.UI.Dynamic;
using UnityEngine;

public class PracticeSettingsModal : ModalDynamic
{
    public PracticeSettingsModal() : base(new Vector2(500, 500))
    {
        
    }

    public override void Create()
    {
        Title.SetText("Practice Settings");
        Description.SetText("Configuration actions for practice mode");
        
        // TODO fix vertical spacing

        MainContent.CreateButton()
            .ApplyTemplate(Button.VerticalLayoutTemplate)
            .StepIntoLabel(label => label.SetText("Reset Robot"))
            .AddOnClickedEvent(b => ModeManager.ResetRobot());

        MainContent.CreateButton()
            .ApplyTemplate(Button.VerticalLayoutTemplate)
            .StepIntoLabel(label => label.SetText("Reset Gamepieces"))
            .AddOnClickedEvent(b => ModeManager.ResetGamepieces());

        MainContent.CreateButton()
            .ApplyTemplate(Button.VerticalLayoutTemplate)
            .StepIntoLabel(label => label.SetText("Reset Field"))
            .AddOnClickedEvent(b => ModeManager.ResetField());

        MainContent.CreateButton()
            .ApplyTemplate(Button.VerticalLayoutTemplate)
            .StepIntoLabel(label => label.SetText("Reset All"))
            .AddOnClickedEvent(b => ModeManager.ResetAll());
    }
    
    public override void Update(){}
    
    public override void Delete(){}
}