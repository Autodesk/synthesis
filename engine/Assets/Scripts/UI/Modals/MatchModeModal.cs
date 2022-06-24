using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.UI.Dynamic
{
    public class MatchModeModal : ModalDynamic
    {
        public MatchModeModal() : base(new Vector2(500, 500)) { }

        public override void Create()
        {
            Title.SetText("Match Mode");
            Description.SetText("Configure Match Mode");

            AcceptButton.AddOnClickedEvent(b => ModalManager.CloseModal()) ;
            CancelButton.AddOnClickedEvent(b => {//need to add in isMatchModalOpen integration
                ModalManager.CloseModal();
                });

            MainContent.CreateLabel().ApplyTemplate(Label.BigLabelTemplate).SetText("Configure Match");
            var allianceSelection = MainContent.CreateDropdown().ApplyTemplate(Dropdown.VerticalLayoutTemplate)
                .StepIntoLabel(l => l.SetText("Select Alliance Color"))
                .SetOptions(new string[] { "Red", "Blue" })
                .AddOnValueChangedEvent((d, i, o) => Debug.Log($"{d.Label.Text} -> [{i}] {o.text}"));
            var robotDropdown = MainContent.CreateDropdown().ApplyTemplate(Dropdown.VerticalLayoutTemplate)
                .StepIntoLabel(l => l.SetText("Select Robot"))
                .SetOptions(new string[] { "2018 Mean Machine", "Spartabots", "Dozer"})
                .AddOnValueChangedEvent((d, i, o) => Debug.Log($"{d.Label.Text} -> [{i}] {o.text}"));
            var fieldDropdown = MainContent.CreateDropdown().ApplyTemplate(Dropdown.VerticalLayoutTemplate)
                .StepIntoLabel(l => l.SetText("Select Game"))
                .SetOptions(new string[] { "2018 First Power Up", "2019 Destination Deep Space" })
                .AddOnValueChangedEvent((d, i, o) => Debug.Log($"{d.Label.Text} -> [{i}] {o.text}"));
            var spawnPosition = MainContent.CreateDropdown().ApplyTemplate(Dropdown.VerticalLayoutTemplate)
                .StepIntoLabel(l => l.SetText("Select Spawn Position"))
                .SetOptions(new string[] { "Left", "Middle", "Right" })
                .AddOnValueChangedEvent((d, i, o) => Debug.Log($"{d.Label.Text} -> [{i}] {o.text}"));
        }

        public override void Delete() {
        }
    }

}