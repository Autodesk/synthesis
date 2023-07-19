using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Synthesis.UI.Dynamic;
using TMPro;
using UnityEngine;
using SynthesisAPI.Utilities;
using Utilities.ColorManager;
using Logger = SynthesisAPI.Utilities.Logger;

#nullable enable

namespace Synthesis.UI.Dynamic {
    public class AddFieldModal : ModalDynamic {
        private string _root;
        private int _selectedIndex = -1;
        private string[] _files;

        public string Folder = "Mira/Fields";

        public AddFieldModal() : base(new Vector2(400, 55)) {}

        public override void Create() {
            _root = ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", Folder), '/');
            if (!Directory.Exists(_root))
                Directory.CreateDirectory(_root);
            _files = Directory.GetFiles(_root).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();

            Title.SetText("Field Selection");

            ModalImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("PlusIcon"))
                .SetColor(ColorManager.SynthesisColor.MainText);

            AcceptButton.StepIntoLabel(label => label.SetText("Load")).AddOnClickedEvent(b => {
                if (_selectedIndex != -1) {
                    FieldSimObject.DeleteField();
                    FieldSimObject.SpawnField(_files[_selectedIndex]);
                    DynamicUIManager.CloseActiveModal();
                }
            });

            var chooseRobotDropdown = MainContent.CreateDropdown()
                                          .SetOptions(_files.Select(x => Path.GetFileName(x)).ToArray())
                                          .AddOnValueChangedEvent((d, i, data) => _selectedIndex = i)
                                          .SetTopStretch<Dropdown>();

            _selectedIndex = _files.Length > 0 ? 0 : -1;
        }

        public override void Update() {}

        public override void Delete() {}

        private string ParsePath(string p, char c) {
            string[] a = p.Split(c);
            string b   = "";
            for (int i = 0; i < a.Length; i++) {
                switch (a[i]) {
                    case "$appdata":
                        b += Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        break;
                    default:
                        b += a[i];
                        break;
                }
                if (i != a.Length - 1)
                    b += Path.AltDirectorySeparatorChar;
            }
            return b;
        }
    }
}