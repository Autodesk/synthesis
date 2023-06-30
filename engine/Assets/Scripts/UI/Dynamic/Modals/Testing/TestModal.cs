using System.Collections;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;
using TMPro;
using System;

namespace Synthesis.UI.Dynamic {
    public class TestModal : ModalDynamic {
        public TestModal() : base(new Vector2(400, 600)) {}

        public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 15f);
            return u;
        };

        public override void Create() {
            // (var left, var right) = base.MainContent.SplitLeftRight(250, 100);
            Title.SetText("Test Modal");
            Description.SetText("This is a test modal for our new Dynamic UI system");
            // .SetHeight<Label>(Description.Size.y / 2f);
            // TODO: Make these do things with the
            AcceptButton.AddOnClickedEvent(b => Debug.Log("Test Modal -> Accept"));
            CancelButton.AddOnClickedEvent(b => Debug.Log("Test Modal -> Cancel"));

            var label = MainContent.CreateLabel()
                            .ApplyTemplate(Label.BigLabelTemplate)
                            .SetText("Main Settings")
                            .SetTopStretch();

            var testRect = label.RootRectTransform.GetOffsetRect();

            var useAnalyticsToggle = MainContent.CreateToggle().ApplyTemplate(VerticalLayout);
            useAnalyticsToggle.AddOnStateChangedEvent((t, s) => { Debug.Log($"{t.TitleLabel.Text}: {s}"); });
            useAnalyticsToggle.TitleLabel.SetText("Use Analytics");

            testRect = useAnalyticsToggle.RootRectTransform.GetOffsetRect();

            var testSlider =
                MainContent.CreateSlider(label: "Zoom Sensitivity", minValue: 1f, maxValue: 10, currentValue: 3)
                    .ApplyTemplate(VerticalLayout);

            var testDropdownMenu = MainContent.CreateLabeledDropdown()
                                       .StepIntoLabel(l => l.SetText("Test Dropdown"))
                                       .StepIntoDropdown(d => d.SetValue(0))
                                       .ApplyTemplate(VerticalLayout);
            // var testButton = MainContent.CreateButton(text: "Join Our
            // Discord").ApplyTemplate(Button.VerticalLayoutTemplate)
            //     .AddOnClickedEvent(b => Debug.Log($"'{b.Label.Text}' Clicked"));
            // var testLabeledButton =
            // MainContent.CreateLabeledButton().ApplyTemplate(LabeledButton.VerticalLayoutTemplate)
            //     .StepIntoButton(b => b.AddOnClickedEvent(x => Debug.Log("Labeled Button Pressed")).StepIntoLabel(l =>
            //     l.SetText("Tab"))) .StepIntoLabel(l => l.SetText("Pause/Play"));
            // // var testDropdown = MainContent.CreateDropdown().ApplyTemplate(Dropdown.VerticalLayoutTemplate)
            // //     .StepIntoLabel(l => l.SetText("Test Dropdown BOII"))
            // //     .SetOptions(new string[] { "Red", "Square", "Candy", "Window", "Pixel" })
            // //     .AddOnValueChangedEvent((d, i, o) => Debug.Log($"{d.Label.Text} -> [{i}] {o.text}"));
            // var testInputField = MainContent.CreateInputField().ApplyTemplate(InputField.VerticalLayoutTemplate)
            //     .StepIntoHint(h => h.SetText("Enter a number..."))
            //     .StepIntoLabel(l => l.SetText("Test Input Field"))
            //     .SetContentType(TMP_InputField.ContentType.IntegerNumber)
            //     .SetValue("486743");
        }

        public override void Update() {}

        public override void Delete() {}
    }
}
