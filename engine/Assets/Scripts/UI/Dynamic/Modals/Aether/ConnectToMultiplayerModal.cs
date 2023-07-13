using Google.Protobuf;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Controller;
using SynthesisAPI.Utilities;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

using Logger                = SynthesisAPI.Utilities.Logger;
using ClientConnectionState = ConnectToMultiplayerMode.ClientConnectionState;
using ClientActionState     = ConnectToMultiplayerMode.ClientActionState;

namespace Synthesis.UI.Dynamic {
    public class ConnectToMultiplayerModal : ModalDynamic {
        private const float MAIN_CONTENT_WIDTH      = 1000.0f;
        private const float MAIN_CONTENT_HEIGHT     = 700.0f;
        private const float REFRESH_CLIENT_INTERVAL = 0.5f;

        private int _selectedRobotIndex     = 0;
        private float _timeSinceLastRefresh = 0.0f;

        private Label _clientConnectionStatus;
        private Label _clientActionStatus;
        private Label _otherInfo;
        private Dropdown _robotDropdown;

        private bool _robotSelected = false;

        private static ConnectToMultiplayerModal _self;
        private ConnectToMultiplayerMode _mode;

        public ConnectToMultiplayerModal() : base(new Vector2(MAIN_CONTENT_WIDTH, MAIN_CONTENT_HEIGHT)) {}

        public override void Create() {
            _mode = (ModeManager.CurrentMode as ConnectToMultiplayerMode)!;

            AcceptButton.RootGameObject.SetActive(false);
            CancelButton.Label.SetText("Disconnect");

            (var left, var right) = MainContent.SplitLeftRight(leftWidth: (MAIN_CONTENT_WIDTH - 20.0f) / 2.0f, 20.0f);
            left.EnsureImage().StepIntoImage(
                i => i.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK_ACCENT)));

            _clientConnectionStatus = left.CreateLabel(30)
                                          .SetTopStretch<Label>(anchoredY: 30.0f)
                                          .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Top)
                                          .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left);

            _clientActionStatus = left.CreateLabel(30)
                                      .SetTopStretch<Label>(anchoredY: 30.0f * 2)
                                      .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Top)
                                      .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left);

            _otherInfo = left.CreateLabel(30)
                             .SetTopStretch<Label>(anchoredY: 30.0f * 3)
                             .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Top)
                             .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left)
                             .SetText("");

            _robotDropdown = right.CreateDropdown()
                                 .SetOptions(_mode.GetAvailableRobots().Select(x => Path.GetFileName(x)).ToArray())
                                 .AddOnValueChangedEvent((d, i, data) => _selectedRobotIndex = i)
                                 .SetTopStretch<Dropdown>();

            right.CreateButton(text: "Choose Robot")
                .SetHeight<Button>(30.0f)
                .SetTopStretch<Button>(anchoredY: 45.0f)
                .AddOnClickedEvent(b => {
                    Logger.Log("Selecting robot", LogLevel.Info);
                    if (!_robotSelected) {
                        try {
                            _mode.SelectRobot(_mode.GetAvailableRobots()[_selectedRobotIndex]);
                        } catch (Exception e) {
                            Logger.Log($"Error selecting robot: {e.Message}", LogLevel.Error);
                            _otherInfo.SetText($"Error selecting robot: {e.Message}");
                            return;
                        }

                        _otherInfo.SetText("Robot selected.");
                        _robotSelected = true;
                    } else {
                        _otherInfo.SetText("Robot is already selected.");
                    }
                });

            right.CreateButton(text: "Refresh robot list")
                .SetHeight<Button>(30.0f)
                .SetTopStretch<Button>(anchoredY: 45.0f * 2)
                .AddOnClickedEvent(b => {
                    Logger.Log("Refreshing robot list", LogLevel.Info);
                    _mode.RequestServerRobotData();
                    _robotDropdown.SetOptions(_mode.GetAvailableRobots().Select(x => Path.GetFileName(x)).ToArray());
                });
        }

        public override void Update() {
            if (Time.realtimeSinceStartup - _timeSinceLastRefresh > REFRESH_CLIENT_INTERVAL) {
                RefreshClientInfo();
            }
        }

        private void RefreshClientInfo() {
            switch (_mode.ConnectionState) {
                case ClientConnectionState.Disconnected:
                    _clientConnectionStatus.SetText("Disconnected from server.");
                    break;
                case ClientConnectionState.Connecting:
                    _clientConnectionStatus.SetText("Connecting to server...");
                    break;
                case ClientConnectionState.Connected:
                    _clientConnectionStatus.SetText("Connected to server.");
                    break;
            }

            switch (_mode.ActionState) {
                case ClientActionState.Idle:
                    _clientActionStatus.SetText("Waiting for server...");
                    break;
                case ClientActionState.UploadingData:
                    _clientActionStatus.SetText("Uploading data...");
                    break;
                case ClientActionState.DownloadingData:
                    _clientActionStatus.SetText("Downloading data...");
                    break;
            }
        }

        public override void Delete() {
            _mode.End();
        }
    }
}
