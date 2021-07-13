using UnityEngine;
using System.IO;
using System;

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

        void Start()
        {
            _root = ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", Folder), '/');
            ShowDirectory(_root);
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
                foreach (string path in Directory.GetDirectories(filePath))//, "*.g*"))
                {
                    Instantiate(addItem, list.transform).GetComponent<AddItem>().Init(path.Substring(_root.Length + Path.DirectorySeparatorChar.ToString().Length),
                        ParsePath(path, '\\'), //apparently it only works if you replace the back slashes with forward slashes
                        /*GetComponent<Panel>()*/ this);
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
