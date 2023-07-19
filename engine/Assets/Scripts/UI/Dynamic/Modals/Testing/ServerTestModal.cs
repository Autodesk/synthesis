using Google.Protobuf.WellKnownTypes;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Controller;
using SynthesisAPI.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.ColorManager;

using Logger = SynthesisAPI.Utilities.Logger;

namespace Synthesis.UI.Dynamic {

    public class ServerTestModal : ModalDynamic {
        private const float REFRESH_CLIENTS_INTERVAL = 0.3f;

        private const float MAIN_CONTENT_WIDTH  = 1000f;
        private const float MAIN_CONTENT_HEIGHT = 700f;

        private static ServerTestModal _self;

        private Label _statusLabel;
        private Button _refreshButton;

        private float _lastRefresh = 0f;

        private ServerTestMode _mode;

        public ServerTestModal() : base(new Vector2(MAIN_CONTENT_WIDTH, MAIN_CONTENT_HEIGHT)) {}

        private float _lastSignalValue = 0f;

        public override void Create() {
            _mode = (ModeManager.CurrentMode as ServerTestMode)!;

            (var left, var right) = MainContent.SplitLeftRight(leftWidth: (MAIN_CONTENT_WIDTH - 20f) / 2, 20f);

            left.EnsureImage().StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.InteractiveBackground));

            _statusLabel = left.CreateLabel(30)
                               .SetStretch<Label>(15f, 15f, 15f, 15f)
                               .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Top)
                               .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left);

            _statusLabel.SetText("Press Button...");

            _refreshButton = right.CreateButton(text: "Refresh")
                                 .SetHeight<Button>(30f)
                                 .SetTopStretch<Button>()
                                 .AddOnClickedEvent(b => { RefreshClientList(); });

            right.CreateButton(text: "Kill")
                .SetHeight<Button>(30f)
                .SetTopStretch<Button>(anchoredY: 45f)
                .AddOnClickedEvent(b => { _mode.KillClient(0); });

            right.CreateButton(text: "Kill All")
                .SetHeight<Button>(30f)
                .SetTopStretch<Button>(anchoredY: 45f * 2f)
                .AddOnClickedEvent(b => { _mode.KillClients(); });

            right.CreateButton(text: "Increment Signal")
                .SetHeight<Button>(30f)
                .SetTopStretch<Button>(anchoredY: 45f * 3f)
                .AddOnClickedEvent(b => {
                    _lastSignalValue += 1;
                    var signals = new List<SignalData> {
                        new() {SignalGuid = "test", Name = "Test Signal", Value = Value.ForNumber(_lastSignalValue)}
                    };
                    _mode.Clients[0]?.UpdateControllableState(signals);
                });

            right.CreateButton(text: "Send Transform")
                .SetHeight<Button>(30f)
                .SetTopStretch<Button>(anchoredY: 45f * 4f)
                .AddOnClickedEvent(b => {
                    _lastSignalValue += 1;
                    ServerTransforms transformData = new ServerTransforms();
                    transformData.Guid             = _mode.Clients[1]?.Guid ?? 0;
                    transformData.Transforms.Add("test", new ServerTransformData());
                    _mode.Clients[1]
                        ?.UpdateTransforms(new List<ServerTransforms> { transformData })
                        .ContinueWith((x, o) => {
                            if (x.Result.isError)
                                Logger.Log("Error");

                            var msg = x.Result.GetResult();
                            msg?.FromControllableStates.AllUpdates.ForEach(
                                y => y.UpdatedSignals.ForEach(z => Logger.Log($"[{z.SignalGuid}] {z.Value}")));
                        }, null);
                });

            right.CreateButton(text: "Send robot data")
                .SetHeight<Button>(30f)
                .SetTopStretch<Button>(anchoredY: 45f * 5f)
                .AddOnClickedEvent(b => {
                    var robotData  = new DataRobot();
                    robotData.Guid = _mode.Clients[1]?.Guid ?? 0;
                    _mode.Clients[1]?.UploadRobotData(robotData).ContinueWith((x, o) => {
                        if (x.Result.isError)
                            Logger.Log("Error");

                        var msg = x.Result.GetResult();
                        Logger.Log($"Received Response: {msg?.FromDataRobot.Guid}");
                    }, null);
                });

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
            var clients  = _mode.ClientInformation;
            string s     = "";
            clients.ForEach(x => s += $"{x}\n");
            _statusLabel.SetText(s == string.Empty ? "Empty..." : s);
        }
    }
}
