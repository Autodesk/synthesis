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
            Title.text = "Test Modal";
            Description.text = "This is a test modal for our new Dynamic UI system";
            AcceptButton.UnityButton.onClick.AddListener(() => Debug.Log("Test Modal -> Accept"));
            CancelButton.UnityButton.onClick.AddListener(() => Debug.Log("Test Modal -> Cancel"));

            MainContent.CreateLabel().ApplyTemplate(Label.BigLabelTemplate).SetText("Main Settings");

            var useAnalyticsToggle = MainContent.CreateToggle().ApplyTemplate(Toggle.VerticalLayoutTemplate);
            useAnalyticsToggle.SetStateChangeCallback(
                (t, s) => {
                    Debug.Log($"{t.TitleLabel.Text}: {s}");
                }
            );
            useAnalyticsToggle.TitleLabel.SetText("Use Analytics");

            var testSlider = MainContent.CreateSlider(label: "Zoom Sensitivity", minValue: 1f, maxValue: 10, currentValue: 3)
                .ApplyTemplate(Slider.VerticalLayoutTemplate);
        }
    }
}
