using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Synthesis.UI.Dynamic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Modals
{
    public class AddRobotModal : ModalDynamic
    {

        private string _root;
        private TMP_Dropdown.OptionData _chosenRobot;

        public string Folder = "Mira";
        
        public AddRobotModal() : base(new Vector2(500, 500)) {
        }

        public override void Create()
        {
            _root = ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", Folder), '/');

            string[] filePaths = GetFileNames(_root);
            
            Title.SetText("Robot Selection");
            Description.SetText("Choose which robot you wish to play as");

            AcceptButton
                .StepIntoLabel(label => label.SetText("Load"))
                .AddOnClickedEvent(b => Debug.Log($"Load Button -> Should load {_chosenRobot.text}"));

            var chooseRobotDropdown = MainContent.CreateDropdown().StepIntoLabel(label => label.SetText("Choose Robot"))
                .SetOptions(filePaths)
                .AddOnValueChangedEvent((d, i, data) => _chosenRobot = data)
                .ApplyTemplate(Dropdown.VerticalLayoutTemplate);
        }
        
        public override void Delete(){}

        private string[] GetFileNames(string filePath)
        {
            string[] fullPaths = Directory.GetFiles(filePath);
            // exclude .DS_Store and other files; someone else can change or remove this
            fullPaths = Array.FindAll(fullPaths, path => path.EndsWith(".mira"));
            return Array.ConvertAll(fullPaths, path => path.Substring(_root.Length + Path.DirectorySeparatorChar.ToString().Length));
        }

        private string ParsePath(string p, char c)
        {
            string[] a = p.Split(c);
            string b = "";
            for (int i = 0; i < a.Length; i++)
            {
                switch (a[i])
                {
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
            // Debug.Log(b);
            return b;
        }
    }
}