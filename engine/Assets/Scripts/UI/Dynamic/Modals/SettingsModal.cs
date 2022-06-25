using System.Collections;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;
using TMPro;
using System;

namespace Synthesis.UI.Dynamic {
    public class SettingsModal : ModalDynamic {
        public SettingsModal() : base(new Vector2(500, 500)) { }

        public override void Create() {
            // (var left, var right) = base.MainContent.SplitLeftRight(250, 100);
            Title.SetText("Settings");
            Description.SetText("Select one of the settings in order to change simulation settings");
            
            AcceptButton.AddOnClickedEvent(b => Debug.Log("Settings Modal -> Accept"));
            CancelButton.AddOnClickedEvent(b => Debug.Log("Settings Modal -> Cancel"));

            MainContent.CreateLabel().ApplyTemplate(Label.BigLabelTemplate).SetText("Main Settings");

            var reportAnalyticsToggle = MainContent.CreateToggle().ApplyTemplate(Toggle.VerticalLayoutTemplate);
            reportAnalyticsToggle.AddOnStateChangedEvent(
                (t, s) => {
                    Debug.Log($"{t.TitleLabel.Text}: {s}");
                }
            );
            reportAnalyticsToggle.TitleLabel.SetText("Report Analytics");

            MainContent.CreateLabel().ApplyTemplate(Label.BigLabelTemplate).SetText("Smaller Label");
            var verticalLookSensitivitySlider = MainContent.CreateSlider(label: "Pitch Sensitivity", minValue: 1f, maxValue: 15f, currentValue: 3)
                .ApplyTemplate(Slider.VerticalLayoutTemplate);

            // var gamerTagInput = MainContent.CreateInputField();
            // configure once InputField is finished

            // should be in scrollview and created dynamically with controls?
            var shootGamepiece = MainContent.CreateLabeledButton().StepIntoLabel(label =>
            {
                label.SetText("Shoot Gamepiece");
            }).StepIntoButton(button =>
            {
                button.Label.SetText("SPACE");
            }).ApplyTemplate(LabeledButton.VerticalLayoutTemplate);

            var websiteButton = MainContent.CreateButton().ApplyTemplate(Button.VerticalLayoutTemplate).StepIntoLabel(
                label =>
                {
                    label.SetText("Visit our Website");
                }).AddOnClickedEvent(b =>
            {
                Debug.Log("Visit website button clicked!");
            });
            
            var viewsDropdown = MainContent.CreateLabeledDropdown()
                .StepIntoLabel(l => l.SetText("View"))
                .StepIntoDropdown(
                    d => d.SetOptions(new string[] { "Bird's Eye View", "Driver Station", "Orbit View", "Free Cam" })
                    .AddOnValueChangedEvent((d, i, o) => Debug.Log($"View -> [{i}] {o.text}")));
        }

        public override void Update() { }

        public override void Delete() { }
    }
}
