using UnityEngine;
using System.IO;

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
            _root = Path.Combine(FileSystem.FileSystem.Root, Folder);
            ShowDirectory(_root);
        }

        public void RefreshFiles()
        {
            foreach (Transform t in list.transform)
                Object.Destroy(t.gameObject);
            ShowDirectory(_root);
        }

        private void ShowDirectory(string filePath)
        {
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
            else if (list != null)
                foreach (string path in Directory.GetFiles(filePath, "*.g*"))
                    Instantiate(addItem, list.transform).GetComponent<AddItem>().Init(path.Substring(_root.Length+ Path.DirectorySeparatorChar.ToString().Length),
                        path, /*GetComponent<Panel>()*/ this);
        }
    }
}
