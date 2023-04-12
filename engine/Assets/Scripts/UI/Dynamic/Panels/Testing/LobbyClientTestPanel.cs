using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Threading.Tasks;

using Logger = SynthesisAPI.Utilities.Logger;
using LogLevel = SynthesisAPI.Utilities.LogLevel;

namespace Synthesis.UI.Dynamic {
    public class LobbyClientTestPanel : PanelDynamic {

        public const string ROBOTO_BOLD = "Roboto-Bold SDF";
        public const string ROBOTO_REGULAR = "Roboto-Regular SDF";

        public LobbyClientTestPanel() : base(new Vector2(200, 100)) { }

        private Label X, Y, Z;

        private Task _connectTask;

        public override bool Create() {

            CancelButton.RootGameObject.SetActive(false);
            AcceptButton.StepIntoLabel(l => l.SetText("Close"));
            AcceptButton.AddOnClickedEvent(b => DynamicUIManager.ClosePanel<RobotDetailsPanel>());

            // var normalFont = SynthesisAssetCollection.GetFont(ROBOTO_REGULAR);
            // Func<Label, Label> nonHighlightedLabel =
            //     l => l.SetFont(normalFont).SetFontSize(14).SetVerticalAlignment(VerticalAlignmentOptions.Middle).SetHorizontalAlignment(HorizontalAlignmentOptions.Left);
            //
            // MainContent.CreateLabel(15f).ApplyTemplate(nonHighlightedLabel).SetTopStretch().SetText("Position");
            // X = MainContent.CreateLabel(15f).ApplyTemplate(nonHighlightedLabel).SetTopStretch(leftPadding: 10f, anchoredY: 15f).SetText("X: 0.0");
            // Y = MainContent.CreateLabel(15f).ApplyTemplate(nonHighlightedLabel).SetTopStretch(leftPadding: 10f, anchoredY: 30f).SetText("Y: 0.0");
            // Z = MainContent.CreateLabel(15f).ApplyTemplate(nonHighlightedLabel).SetTopStretch(leftPadding: 10f, anchoredY: 45f).SetText("Z: 0.0");

            var inst = NetManager.ClientManager.Instance;
            _connectTask = NetManager.ClientManager.Instance.Connect();
        
            return true;
        }

        
        public override void Update() {
            if (_connectTask != null && _connectTask.Status == TaskStatus.RanToCompletion) {
                Logger.Log("Done Connecting", LogLevel.Debug);
                _connectTask = null;
            }
        }

        public override void Delete() { }
    }
}
