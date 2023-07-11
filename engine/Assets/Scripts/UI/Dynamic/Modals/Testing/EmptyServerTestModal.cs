using Synthesis.UI.Dynamic;
using SynthesisAPI.Controller;
using SynthesisAPI.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Logger = SynthesisAPI.Utilities.Logger;

namespace Synthesis.UI.Dynamic {
    public class EmptyServerTestModal : ModalDynamic {
        private const float REFRESH_CLIENTS_INTERVAL = 0.3f;

        private const float MAIN_CONTENT_WIDTH  = 1000.0f;
        private const float MAIN_CONTENT_HEIGHT = 700.0f;

        private float _lastRefresh = 0.0f;

        private Label _statusLabel;

        private static EmptyServerTestModal _self;
        private EmptyServerTestMode _mode;

        public EmptyServerTestModal() : base(new Vector2(MAIN_CONTENT_WIDTH, MAIN_CONTENT_HEIGHT)) {}

        public override void Create() {
            _mode = (ModeManager.CurrentMode as EmptyServerTestMode)!;

            (var left, var right) = MainContent.SplitLeftRight(leftWidth: (MAIN_CONTENT_WIDTH - 20.0f) / 2.0f, 20.0f);
            left.EnsureImage().StepIntoImage(
                i => i.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK_ACCENT)));

            _statusLabel = left.CreateLabel(30)
                               .SetStretch<Label>(15.0f, 15.0f, 15.0f, 15.0f)
                               .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Top)
                               .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left);

            _statusLabel.SetText("Waiting for server...");

            right.CreateButton(text: "Kill").SetHeight<Button>(30.0f).SetTopStretch<Button>().AddOnClickedEvent(b => {
                _mode.KillServer();
                _statusLabel.SetText("Server killed.");
            });
        }

        public override void Update() {
            if (Time.realtimeSinceStartup - _lastRefresh > REFRESH_CLIENTS_INTERVAL) {
                RefreshClientList();
            }
        }

        public override void Delete() {}

        private void RefreshClientList() {
            if (!(_mode.IsServerAlive)) {
                return;
            }

            _lastRefresh = Time.realtimeSinceStartup;
            var clients  = _mode.ClientInformation;
            string s     = "";
            clients.ForEach(x => s += $"{x}\n");
            _statusLabel.SetText(s == string.Empty ? "Waiting for clients..." : s);
        }
    }
}
