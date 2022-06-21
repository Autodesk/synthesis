using System.Collections;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;
using TMPro;
using System;

namespace Synthesis.UI.Dynamic {
    public class TestModal : ModalDynamic {
        public TestModal() : base(new Vector2(500, 500)) { }

        public override void Create() {
            // (var left, var right) = base.MainContent.SplitLeftRight(250, 100);
            Title.SetText("Test Modal");
            Description.SetText("This is a test modal for our new Dynamic UI system");
                // .SetHeight<Label>(Description.Size.y / 2f);
            // TODO: Make these do things with the
            AcceptButton.AddOnClickedEvent(b => Debug.Log("Test Modal -> Accept"));
            CancelButton.AddOnClickedEvent(b => Debug.Log("Test Modal -> Cancel"));

            MainContent.CreateLabel().ApplyTemplate(Label.BigLabelTemplate).SetText("Main Settings");

            var useAnalyticsToggle = MainContent.CreateToggle().ApplyTemplate(Toggle.VerticalLayoutTemplate);
            useAnalyticsToggle.AddOnStateChangedEvent(
                (t, s) => {
                    Debug.Log($"{t.TitleLabel.Text}: {s}");
                }
            );
            useAnalyticsToggle.TitleLabel.SetText("Use Analytics");

            var testSlider = MainContent.CreateSlider(label: "Zoom Sensitivity", minValue: 1f, maxValue: 10, currentValue: 3)
                .ApplyTemplate(Slider.VerticalLayoutTemplate);

            var testButton = MainContent.CreateButton(text: "Join Our Discord").ApplyTemplate(Button.VerticalLayoutTemplate)
                .AddOnClickedEvent(b => Debug.Log($"'{b.Label.Text}' Clicked"));
            var testLabeledButton = MainContent.CreateLabeledButton().ApplyTemplate(LabeledButton.VerticalLayoutTemplate)
                .StepIntoButton(b => b.AddOnClickedEvent(x => Debug.Log("Labeled Button Pressed")).StepIntoLabel(l => l.SetText("Tab")))
                .StepIntoLabel(l => l.SetText("Pause/Play"));
            var testDropdown = MainContent.CreateDropdown().ApplyTemplate(Dropdown.VerticalLayoutTemplate)
                .StepIntoLabel(l => l.SetText("Test Dropdown BOII"))
                .SetOptions(new string[] { "Red", "Square", "Candy", "Window", "Pixel" })
                .AddOnValueChangedEvent((d, i, o) => Debug.Log($"{d.Label.Text} -> [{i}] {o.text}"));
        }

        public override void Delete() { }
    }
}
