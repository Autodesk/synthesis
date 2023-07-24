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
            Title.SetText("Test Modal");
            Description.SetText("This is a test modal for our new Dynamic UI system");
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
        }

        public override void Update() {}

        public override void Delete() {}
    }
}
