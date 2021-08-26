using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;
using Synthesis.UI.Bars;
using TMPro;

namespace Synthesis.UI.Panels.Variant
{
    public class AddPanel : Panel
    {
        [SerializeField]
        public GameObject list;

        [SerializeField]
        public GameObject addItem;

        [SerializeField]
        public string Folder;

        private string _root;

        private static AddItem _currentSelection;

        public NavigationBar NavBar;
        public Image LoadButton;

        private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private Color enabledColor = new Color(0.1294118f, 0.5882353f, 0.9529412f, 1f);

        public void SelectItem(AddItem item)
        {
            if (_currentSelection != null) { 
                _currentSelection.Lighten();
            }
            else
            {
                GameObject.Find("Load-Item-Button").GetComponent<Image>().color = enabledColor;
            }
            _currentSelection = item;
            _currentSelection.Darken();
        }

        public void ClearSelection()
        {
            if (_currentSelection != null) _currentSelection.Lighten();
            _currentSelection = null;
            LoadButton.color = disabledColor;
        }

        public void LoadRobot()
        {
            if (_currentSelection != null)
            {
                _currentSelection.AddModel();
                NavBar.CloseAllPanels();
            }
        }
        
        public void LoadField()
        {
            if (_currentSelection != null)
            {
                _currentSelection.AddField();
                NavBar.CloseAllPanels();
            }
        }


        void Start()
        {
            _root = ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", Folder), '/');
            ShowDirectory(_root);
            LoadButton.color = disabledColor;
            _currentSelection = null;
        }

        public void RefreshFiles()
        {
            foreach (Transform t in list.transform)
                UnityEngine.Object.Destroy(t.gameObject);
            ShowDirectory(_root);
        }

        private void ShowDirectory(string filePath)
        {
            //log, find items, etc
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            else if (list != null)
            {
                foreach (string path in Directory.GetFiles(filePath))//TRANSLATED FILE (SPR)
                {
                    Instantiate(addItem, list.transform).GetComponent<AddItem>().Init(path.Substring(_root.Length + Path.DirectorySeparatorChar.ToString().Length),
                        ParsePath(path, '\\'));
                }
                foreach (string path in Directory.GetDirectories(filePath))//LEGACY FORMAT
                {
                    Instantiate(addItem, list.transform).GetComponent<AddItem>().Init(path.Substring(_root.Length + Path.DirectorySeparatorChar.ToString().Length),
                        ParsePath(path, '\\'));
                }
            }

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
