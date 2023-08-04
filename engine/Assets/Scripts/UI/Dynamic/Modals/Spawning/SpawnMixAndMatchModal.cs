using System.IO;
using System.Linq;
using SimObjects.MixAndMatch;
using SynthesisAPI.Utilities;
using Synthesis.UI.Dynamic;
using UnityEngine;
using Utilities.ColorManager;
using Logger = SynthesisAPI.Utilities.Logger;

#nullable enable

namespace UI.Dynamic.Modals.Spawning {
    public class SpawnMixAndMatchModal : ModalDynamic {
        private int _selectedIndex = -1;
        private string[] _files;


        public SpawnMixAndMatchModal() : base(new Vector2(400, 55)) {}

        public override void Create() {
            _files = MixAndMatchSaveUtil.RobotFiles;

            Title.SetText("Mix And Match Selection");

            ModalIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("plus"))
                .SetColor(ColorManager.SynthesisColor.MainText);

            AcceptButton.StepIntoLabel(label => label.SetText("Load")).AddOnClickedEvent(b => {
                if (_selectedIndex != -1) {
                    RobotSimObject.SpawnRobot(MixAndMatchSaveUtil.LoadRobotData(_files[_selectedIndex]));

                    DynamicUIManager.CloseActiveModal();
                    RobotSimObject.GetCurrentlyPossessedRobot()?.CreateDrivetrainTooltip();
                }
            });
            var chooseRobotDropdown = MainContent.CreateDropdown()
                                          .SetOptions(_files.Select(x => Path.GetFileName(x)).ToArray())
                                          .AddOnValueChangedEvent((d, i, data) => _selectedIndex = i)
                                          .SetTopStretch<Dropdown>();

            _selectedIndex = _files.Length > 0 ? 0 : -1;
        }

        public override void Update() {
            if (RobotSimObject.SpawnedRobots.Count >= RobotSimObject.MAX_ROBOTS) {
                Logger.Log("Maximum number of bots reached", LogLevel.Info);
                DynamicUIManager.CloseActiveModal();
            }
        }

        public override void Delete() {}

        public static string ParsePath(string p, char c) {
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