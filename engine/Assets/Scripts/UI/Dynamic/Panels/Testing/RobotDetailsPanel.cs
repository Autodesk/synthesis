using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace Synthesis.UI.Dynamic {
    public class RobotDetailsPanel : PanelDynamic {
        public const string ROBOTO_BOLD    = "Roboto-Bold SDF";
        public const string ROBOTO_REGULAR = "Roboto-Regular SDF";

        public RobotDetailsPanel() : base(new Vector2(200, 100)) {}

        private Label X, Y, Z;

        public override bool Create() {
            CancelButton.RootGameObject.SetActive(false);
            AcceptButton.StepIntoLabel(l => l.SetText("Close"));
            AcceptButton.AddOnClickedEvent(b => DynamicUIManager.ClosePanel<RobotDetailsPanel>());

            var normalFont                         = SynthesisAssetCollection.GetFont(ROBOTO_REGULAR);
            Func<Label, Label> nonHighlightedLabel = l => l.SetFont(normalFont)
                                                              .SetFontSize(14)
                                                              .SetVerticalAlignment(VerticalAlignmentOptions.Middle)
                                                              .SetHorizontalAlignment(HorizontalAlignmentOptions.Left);

            MainContent.CreateLabel(15f).ApplyTemplate(nonHighlightedLabel).SetTopStretch().SetText("Position");
            X = MainContent.CreateLabel(15f)
                    .ApplyTemplate(nonHighlightedLabel)
                    .SetTopStretch(leftPadding: 10f, anchoredY: 15f)
                    .SetText("X: 0.0");
            Y = MainContent.CreateLabel(15f)
                    .ApplyTemplate(nonHighlightedLabel)
                    .SetTopStretch(leftPadding: 10f, anchoredY: 30f)
                    .SetText("Y: 0.0");
            Z = MainContent.CreateLabel(15f)
                    .ApplyTemplate(nonHighlightedLabel)
                    .SetTopStretch(leftPadding: 10f, anchoredY: 45f)
                    .SetText("Z: 0.0");

            return true;
        }

        public override void Update() {
            Vector3 robotPosition = new Vector3();
            if (RobotSimObject.CurrentlyPossessedRobot != string.Empty) {
                robotPosition = RobotSimObject.GetCurrentlyPossessedRobot().GroundedNode.transform.position;
            }

            X.SetText($"X: {Math.Round(robotPosition.x, 1)}");
            Y.SetText($"Y: {Math.Round(robotPosition.y, 1)}");
            Z.SetText($"Z: {Math.Round(robotPosition.z, 1)}");
        }

        public override void Delete() {}
    }
}
