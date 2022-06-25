using Synthesis.ModelManager;
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

        MainContent.CreateButton().StepIntoLabel(label => label.SetText("Reset Robot"))
            .AddOnClickedEvent(b => ModeManager.ResetRobot())
            .ApplyTemplate(Button.VerticalLayoutTemplate);

        MainContent.CreateButton().StepIntoLabel(label => label.SetText("Reset Gamepieces"))
            .AddOnClickedEvent(b => ModeManager.ResetGamepieces())
            .ApplyTemplate(Button.VerticalLayoutTemplate);

        MainContent.CreateButton().StepIntoLabel(label => label.SetText("Reset Field"))
            .AddOnClickedEvent(b => ModeManager.ResetField())
            .ApplyTemplate(Button.VerticalLayoutTemplate);
    }
    
    public override void Update(){}
    
    public override void Delete(){}
}