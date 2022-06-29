using Synthesis.UI.Dynamic;
using UnityEngine;

public class MatchModeModal : ModalDynamic
{
    public MatchModeModal() : base(new Vector2(500, 500)) { }

    public override void Create()
    {
        Title.SetText("Match Mode");
        Description.SetText("Configure Match Mode");

        MainContent.CreateLabel().ApplyTemplate(Label.BigLabelTemplate).SetText("Configure Match");
        var allianceSelectionLabel = MainContent.CreateLabel().ApplyTemplate(Label.VerticalLayoutTemplate)
            .SetText("Select Alliance Color");
        var allianceSelection = MainContent.CreateDropdown().ApplyTemplate(Dropdown.VerticalLayoutTemplate)
            .SetOptions(new string[] { "Red", "Blue" })
            .AddOnValueChangedEvent((d, i, o) => Debug.Log($"{d.Label.Text} -> [{i}] {o.text}"));
        var robotDropdownLabel = MainContent.CreateLabel().ApplyTemplate(Label.VerticalLayoutTemplate)
            .SetText("Select Robot");
        var robotDropdown = MainContent.CreateDropdown().ApplyTemplate(Dropdown.VerticalLayoutTemplate)
            .SetOptions(new string[] { "2018 Mean Machine", "Spartabots", "Dozer"})
            .AddOnValueChangedEvent((d, i, o) => Debug.Log($"{d.Label.Text} -> [{i}] {o.text}"));
        var fieldDropdownLabel = MainContent.CreateLabel().ApplyTemplate(Label.VerticalLayoutTemplate)
            .SetText("Select Game");
        var fieldDropdown = MainContent.CreateDropdown().ApplyTemplate(Dropdown.VerticalLayoutTemplate)
            .SetOptions(new string[] { "2018 First Power Up", "2019 Destination Deep Space" })
            .AddOnValueChangedEvent((d, i, o) => Debug.Log($"{d.Label.Text} -> [{i}] {o.text}"));
        var spawnPositionLabel = MainContent.CreateLabel().ApplyTemplate(Label.VerticalLayoutTemplate)
            .SetText("Select Spawn Position");
        var spawnPosition = MainContent.CreateDropdown().ApplyTemplate(Dropdown.VerticalLayoutTemplate)
            .SetOptions(new string[] { "Left", "Middle", "Right" })
            .AddOnValueChangedEvent((d, i, o) => Debug.Log($"{d.} -> [{i}] {o.text}"));
    }
    
    public override void Update() {}

    public override void Delete() {
    }
}