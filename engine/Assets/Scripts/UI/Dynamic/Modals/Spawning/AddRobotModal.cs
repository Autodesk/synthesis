using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Synthesis.UI.Dynamic;
using TMPro;
using UnityEngine;

#nullable enable

namespace Synthesis.UI.Dynamic {
    public class AddRobotModal : ModalDynamic {

        private string _root;
        private int _selectedIndex = -1;
        private string[] _files;

        public string Folder = "Mira";
        
        public AddRobotModal() : base(new Vector2(400, 40)) { }

        public override void Create() {
            _root = ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", Folder), '/');
            _files = Directory.GetFiles(_root).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();
            
            Title.SetText("Robot Selection");
            Description.SetText("Choose which robot you wish to play as");

            AcceptButton
                .StepIntoLabel(label => label.SetText("Load"))
                .AddOnClickedEvent(b => {
                    if (_selectedIndex != -1) {
                        RobotSimObject.SpawnRobot(_files[_selectedIndex]);
                        // ItemAnalytics("Robot");
                        DynamicUIManager.CloseActiveModal();
                    }
                });

            var chooseRobotDropdown = MainContent.CreateDropdown()
                .SetOptions(_files.Select(x => Path.GetFileName(x)).ToArray())
                .AddOnValueChangedEvent((d, i, data) => _selectedIndex = i)
                .SetTopStretch<Dropdown>();

            _selectedIndex = _files.Length > 0 ? 0 : -1;

            // MainContent.CreateLabeledButton().SetTopStretch<LabeledButton>(anchoredY: 50).StepIntoLabel(l => l.SetText("Test"));
        }

        public override void Update() { }
        
        public override void Delete() {}

        // private string[] GetFiles(string filePath) {
        //     string[] fullPaths = Directory.GetFiles(filePath);
        //     // exclude .DS_Store and other files; someone else can change or remove this
        //     fullPaths = Array.FindAll(fullPaths, path => path.EndsWith(".mira"));
        //     return Array.ConvertAll(fullPaths, path => path.Substring(_root.Length + Path.DirectorySeparatorChar.ToString().Length));
        // }

        public static string ParsePath(string p, char c)
        {
            string[] a = p.Split(c);
            string b = "";
            for (int i = 0; i < a.Length; i++)
            {
                switch (a[i])
                {
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
            // Debug.Log(b);
            return b;
        }
    }
}