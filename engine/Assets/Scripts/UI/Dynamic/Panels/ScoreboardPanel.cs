using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace Synthesis.UI.Dynamic {
    public class ScoreboardPanel : PanelDynamic {

        public const string ROBOTO_BOLD = "Roboto-Bold SDF";
        public const string ROBOTO_REGULAR = "Roboto-Regular SDF";

        public ScoreboardPanel() : base(new Vector2(170, 100)) { }

        private Label time, redScore, blueScore;

        public override void Create() {

            CancelButton.RootGameObject.SetActive(false);
            
            Title.SetText("Scoreboard");

            var normalFont = SynthesisAssetCollection.GetFont(ROBOTO_REGULAR);
            Func<Label, Label> nonHighlightedLabel =
                l => l.SetFont(normalFont).SetFontSize(20).SetVerticalAlignment(VerticalAlignmentOptions.Middle).SetHorizontalAlignment(HorizontalAlignmentOptions.Left);

            time = MainContent.CreateLabel(15f).ApplyTemplate(nonHighlightedLabel).SetTopStretch(leftPadding: 10f, anchoredY: 15f).SetText(targetTime.ToString());
            redScore = MainContent.CreateLabel(15f).ApplyTemplate(nonHighlightedLabel).SetTopStretch(leftPadding: 10f, anchoredY: 30f).SetText("R: 0");
            blueScore = MainContent.CreateLabel(15f).ApplyTemplate(nonHighlightedLabel).SetTopStretch(leftPadding: 10f, anchoredY: 45f).SetText("B: 0");
        }
        
        float targetTime = 135;
        bool matchEnd = false;
        public override void Update() {
            if(GizmoManager.currentGizmo == null && targetTime >= 0)
            {
                targetTime -= Time.deltaTime;
                time.SetText(Mathf.RoundToInt(targetTime).ToString());
                redScore.SetText($"R: {TempScoreManager.redScore}");
                blueScore.SetText($"B: {TempScoreManager.blueScore}");
            }    
            else if (!matchEnd)
            {
                //end match
                matchEnd = true;
            }
        }

        public override void Delete() { }
    }
}
