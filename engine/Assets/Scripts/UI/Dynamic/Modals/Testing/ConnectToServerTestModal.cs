using Google.Protobuf;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Controller;
using SynthesisAPI.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using Logger = SynthesisAPI.Utilities.Logger;

namespace Synthesis.UI.Dynamic {
    public class ConnectToServerTestModal : ModalDynamic {
        private const float MAIN_CONTENT_WIDTH  = 1000.0f;
        private const float MAIN_CONTENT_HEIGHT = 700.0f;

        private string[] _robotFiles;
        private int _selectedRobotIndex = -1;

        private Label _statusLabel;

        private static ConnectToServerTestModal _self;
        private ConnectToServerTestMode _mode;

        public ConnectToServerTestModal() : base(new Vector2(MAIN_CONTENT_WIDTH, MAIN_CONTENT_HEIGHT)) {}

        public override void Create() {
            string root = ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", "Mira"), '/');
            if (!Directory.Exists(root)) {
                Directory.CreateDirectory(root);
            }

            _robotFiles = Directory.GetFiles(root).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();

            _mode = (ModeManager.CurrentMode as ConnectToServerTestMode)!;

            (var left, var right) = MainContent.SplitLeftRight(leftWidth: (MAIN_CONTENT_WIDTH - 20.0f) / 2.0f, 20.0f);
            left.EnsureImage().StepIntoImage(
                i => i.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK_ACCENT)));

            _statusLabel = left.CreateLabel(30)
                               .SetStretch<Label>(15.0f, 15.0f, 15.0f, 15.0f)
                               .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Top)
                               .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left);

            _statusLabel.SetText("Connecting to server...");

            right.CreateButton(text: "Kill client")
                .SetHeight<Button>(30.0f)
                .SetTopStretch<Button>()
                .AddOnClickedEvent(b => {
                    _mode.KillClient();
                    _statusLabel.SetText("Client killed.");
                });

            right.CreateDropdown()
                .SetOptions(_robotFiles.Select(x => Path.GetFileName(x)).ToArray())
                .AddOnValueChangedEvent((d, i, data) => _selectedRobotIndex = i)
                .SetTopStretch<Dropdown>();

            right.CreateButton(text: "Send Robot Data")
                .SetHeight<Button>(30.0f)
                .SetTopStretch<Button>(anchoredY: 45.0f)
                .AddOnClickedEvent(b => {
                    if (_selectedRobotIndex == -1) {
                        return;
                    }

                    string robotPath = _robotFiles[_selectedRobotIndex];
                    _statusLabel.SetText("Robot data sent...");
                    var robot = new DataRobot { Name = Path.GetFileNameWithoutExtension(robotPath),
                        Data                         = ByteString.CopyFrom(File.ReadAllBytes(robotPath)) };

                    _mode.UploadRobotData(robot).ContinueWith(t => {
                        if (t.IsFaulted) {
                            Logger.Log("Failed to upload robot data", LogLevel.Error);
                        } else {
                            Logger.Log("Robot data uploaded", LogLevel.Info);
                        }
                    });
                });

            right.CreateButton(text: "Request Robot Data")
                .SetHeight<Button>(30.0f)
                .SetTopStretch<Button>(anchoredY: 90.0f)
                .AddOnClickedEvent(b => { _mode.RequestServerRobotData(); });
        }

        public override void Update() {
            var robots = _mode.Robots;

            if (robots.Count > 0) {
                string status = "";
                robots.ForEach((x => status += $"{x.Name}\n"));
                _statusLabel.SetText(status);
            }
        }

        public override void Delete() {}

        private static string ParsePath(string p, char c) {
            string[] a = p.Split(c);
            string b   = "";
            for (int i = 0; i < a.Length; i++) {
                switch (a[i]) {
                    case "$appdata":
                        b += System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                        break;
                    default:
                        b += a[i];
                        break;
                }
                if (i != a.Length - 1)
                    b += System.IO.Path.AltDirectorySeparatorChar;
            }

            return b;
        }
    }
}
