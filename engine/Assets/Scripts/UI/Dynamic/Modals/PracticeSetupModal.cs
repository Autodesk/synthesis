using Synthesis.UI.Dynamic;
using UnityEngine;

public class PracticeSetupModal : ModalDynamic
{
    public PracticeSetupModal() : base(new Vector2(500, 500))
    {
        
    }

    public override void Create()
    {
        Title.SetText("Practice Mode");
        Description.SetText("Configuration for practice mode.");

        var robotDropdown = MainContent.CreateDropdown()
            .SetOptions(new[] { "Robot 1", "Robot 2", "Robot 3" })
            .ApplyTemplate(Dropdown.VerticalLayoutTemplate);

        var fieldDropdown = MainContent.CreateDropdown()
            .SetOptions(new[] { "Field 1", "Field 2", "Field 3" })
            .ApplyTemplate(Dropdown.VerticalLayoutTemplate);
    }
    
    public override void Update() {}

    public override void Delete()
    {
    }
}