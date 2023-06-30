using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Utilities;
using TMPro;
using UnityEngine;
using Logger = SynthesisAPI.Utilities.Logger;

#nullable enable

namespace Synthesis.UI.Dynamic {
    public class AddRobotModal : ModalDynamic {
        private string _root;
        private int _selectedIndex = -1;
        private string[] _files;

        public string Folder = "Mira";

        public AddRobotModal() : base(new Vector2(400, 40)) {}

        public override void Create() {
            _root = ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", Folder), '/');
            if (!Directory.Exists(_root))
                Directory.CreateDirectory(_root);
            _files = Directory.GetFiles(_root).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();

            Title.SetText("Robot Selection");
            Description.SetText("Choose which robot you wish to play as");
            ModalImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("PlusIcon"))
                .SetColor(ColorManager.SYNTHESIS_WHITE);

            AcceptButton.StepIntoLabel(label => label.SetText("Load")).AddOnClickedEvent(b => {
                if (_selectedIndex != -1) {
                    RobotSimObject.SpawnRobot(_files[_selectedIndex]);
                    DynamicUIManager.CloseActiveModal();
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