using System.Collections;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;

namespace Synthesis.UI.Dynamic {

	public class ServerTestModal : ModalDynamic {

		private const float REFRESH_CLIENTS_INTERVAL = 0f;

		private static ServerTestModal _self;

		private Label _statusLabel;
		private Button _refreshButton;

		private float _lastRefresh = 0f;

		public ServerTestModal() : base(new Vector2(500, 500)) { }

		public override void Create() {
			_statusLabel = MainContent.CreateLabel(30).SetStretch<Label>(bottomPadding: 50)
				.SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Top)
				.SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left);

			_statusLabel.SetText("Press Button...");

			_refreshButton = MainContent.CreateButton(text: "Refresh").SetHeight<Button>(50).SetBottomStretch<Button>(30)
				.AddOnClickedEvent(b => {
					RefreshClientList();
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
			var clients = (ModeManager.CurrentMode as ServerTestMode)!.ClientInformation;
			string s = "";
			clients.ForEach(x => s += $"{x}\n");
			_statusLabel.SetText(s);
		}

	}
}
