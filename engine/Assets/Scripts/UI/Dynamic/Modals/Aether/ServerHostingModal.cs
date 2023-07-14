using Synthesis.UI.Dynamic;
using SynthesisAPI.Controller;
using SynthesisAPI.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Logger = SynthesisAPI.Utilities.Logger;

namespace Synthesis.UI.Dynamic {
    public class ServerHostingModal : ModalDynamic {
        private const float REFRESH_CLIENTS_INTERVAL = 0.3f;

        private const float MAIN_CONTENT_WIDTH  = 1000.0f;
        private const float MAIN_CONTENT_HEIGHT = 700.0f;

        private float _lastRefresh = 0.0f;

        private Label _statusLabel;

        private static ServerHostingModal _self;
        private ServerHostingMode _mode;

        public ServerHostingModal() : base(new Vector2(MAIN_CONTENT_WIDTH, MAIN_CONTENT_HEIGHT)) {}

        public override void Create() {
            _mode = (ModeManager.CurrentMode as ServerHostingMode)!;

            Title.SetText("Server Hosting");
            Description.SetText("Host a server for other players to join.");

            AcceptButton.RootGameObject.SetActive(false);
            CancelButton.Label.SetText("Close");
            CancelButton.AddOnClickedEvent(b => {
                _mode.KillServer();
                DynamicUIManager.CloseActiveModal();
            });

            _statusLabel = MainContent.CreateLabel(30)
                               .SetStretch<Label>(15.0f, 15.0f, 15.0f, 15.0f)
                               .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Top)
                               .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left)
                               .SetText("Waiting for clients...");
        }

        public override void Update() {
            if (Time.realtimeSinceStartup - _lastRefresh > REFRESH_CLIENTS_INTERVAL) {
                RefreshClientList();
            }
        }

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

        public override void Delete() {}
    }
}
