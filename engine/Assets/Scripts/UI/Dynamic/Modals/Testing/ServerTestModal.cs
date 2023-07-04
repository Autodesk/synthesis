using System.Collections;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;

namespace Synthesis.UI.Dynamic {

    public class ServerTestModal : ModalDynamic {
        private const float REFRESH_CLIENTS_INTERVAL = 0.05f;

        private const float MAIN_CONTENT_WIDTH  = 1000f;
        private const float MAIN_CONTENT_HEIGHT = 700f;

        private static ServerTestModal _self;

        private Label _statusLabel;
        private Button _refreshButton;

        private float _lastRefresh = 0f;

        public ServerTestModal() : base(new Vector2(MAIN_CONTENT_WIDTH, MAIN_CONTENT_HEIGHT)) {}

        public override void Create() {
            (var left, var right) = MainContent.SplitLeftRight(leftWidth: (MAIN_CONTENT_WIDTH - 20f) / 2, 20f);
            left.EnsureImage().StepIntoImage(
                i => i.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK_ACCENT)));

            _statusLabel = left.CreateLabel(30)
                               .SetStretch<Label>(15f, 15f, 15f, 15f)
                               .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Top)
                               .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left);

            _statusLabel.SetText("Press Button...");

            _refreshButton = right.CreateButton(text: "Refresh")
                                 .SetHeight<Button>(30f)
                                 .SetTopStretch<Button>()
                                 .AddOnClickedEvent(b => { RefreshClientList(); });

            right.CreateButton(text: "Kill").SetHeight<Button>(30f).SetTopStretch<Button>(anchoredY: 45f).AddOnClickedEvent(b => {
                (ModeManager.CurrentMode as ServerTestMode)!.KillClient(0);
            });

            right.CreateButton(text: "Kill All").SetHeight<Button>(30f).SetTopStretch<Button>(anchoredY: 45f * 2f).AddOnClickedEvent(b => {
                (ModeManager.CurrentMode as ServerTestMode)!.KillClients();
			});

            RefreshClientList();

            _self = this;
        }

        public override void Update() {
            if (Time.realtimeSinceStartup - _lastRefresh > REFRESH_CLIENTS_INTERVAL) {
                RefreshClientList();
            }
        }

        public override void Delete() {
            _statusLabel = null;
        }

        private void RefreshClientList() {
            _lastRefresh = Time.realtimeSinceStartup;
            var clients  = (ModeManager.CurrentMode as ServerTestMode)!.ClientInformation;
            string s     = "";
            clients.ForEach(x => s += $"{x}\n");
            _statusLabel.SetText(s == string.Empty ? "Empty..." : s);
        }
    }
}
