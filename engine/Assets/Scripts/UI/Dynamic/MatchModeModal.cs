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
        var allianceSelection = MainContent.CreateLabeledDropdown()
            .StepIntoLabel(l =>
            {
                l.SetText("Select Alliance")
                    .ApplyTemplate(Label.VerticalLayoutTemplate);
            })
            .StepIntoDropdown(d =>
            {
                d.SetOptions(new string[] { "Red", "Blue" })
                    .AddOnValueChangedEvent((d, i, o) => Debug.Log($"{d} -> [{i}] {o.text}"));
            });
        var robotDropdown = MainContent.CreateLabeledDropdown()
            .StepIntoLabel(l =>
            {
                l.SetText("Select Robot")
                    .ApplyTemplate(Label.VerticalLayoutTemplate);
            })
            .StepIntoDropdown(d =>
            {
                d.ApplyTemplate(Dropdown.VerticalLayoutTemplate)
                    .SetOptions(new string[] { "2018 Mean Machine", "Spartabots", "Dozer" })
                    .AddOnValueChangedEvent((d, i, o) => Debug.Log($"{d} -> [{i}] {o.text}"));
            });
        var fieldDropdown = MainContent.CreateLabeledDropdown()
            .StepIntoLabel(l =>
            {
                l.SetText("Select Game")
                    .ApplyTemplate(Label.VerticalLayoutTemplate);
            })
            .StepIntoDropdown(d =>
            {
                d.ApplyTemplate(Dropdown.VerticalLayoutTemplate)
                    .SetOptions(new string[] { "2018 First Power Up", "2019 Destination Deep Space" })
                    .AddOnValueChangedEvent((d, i, o) => Debug.Log($"{d} -> [{i}] {o.text}"));
            });
        var spawnPosition = MainContent.CreateLabeledDropdown()
            .StepIntoLabel(l =>
            {
                l.SetText("Select Spawn Position")
                    .ApplyTemplate(Label.VerticalLayoutTemplate);
            })
            .StepIntoDropdown(d =>
            {
                d.ApplyTemplate(Dropdown.VerticalLayoutTemplate)
                    .SetOptions(new string[] { "Left", "Middle", "Right" })
                    .AddOnValueChangedEvent((d, i, o) => Debug.Log($"{d} -> [{i}] {o.text}"));
            });
    }
    
    public override void Update() {}

    public override void Delete() {
    }
}