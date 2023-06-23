using System;
using System.Runtime.InteropServices;
using Synthesis.UI.Hierarchy.HierarchyItems;
using UnityEngine;
using Synthesis.UI.ContextMenus;
using Synthesis.Util;

using ContextMenu = Synthesis.UI.ContextMenus.ContextMenu;

namespace Synthesis.UI.Hierarchy {
    public class Hierarchy : MonoBehaviour {
        public static Hierarchy HierarchyInstance { get; private set; }

        public HierarchyFolderItem rootFolder;
        public static HierarchyFolderItem RootFolder => HierarchyInstance.rootFolder;
        public GameObject folderPrefab;
        public static GameObject FolderPrefab => HierarchyInstance.folderPrefab;
        public GameObject itemPrefab;
        public static GameObject ItemPrefab => HierarchyInstance.itemPrefab;
        public GameObject contentContainer;
        public static GameObject ContentContainer => HierarchyInstance.contentContainer;
        public static bool Changes                 = true;

        public float TabSize = 20f;
        public float Padding = 2.5f;

        private void Awake() {
            HierarchyInstance = this;
        }

        public void Start() {
            // ContextMenu.Show()
            // ContextMenu.Show(new Vector2(500, -500), "Test Menu", new string[]{"Hello", "There"});

            rootFolder.Init("Scene", null);

            var robots = rootFolder.CreateFolder("Robots");
            var fields = rootFolder.CreateFolder("Fields");

            fields.CreateItem("2020 Infinite Recharge");
        }

        public void Update() {
            if (Changes) {
                Canvas.ForceUpdateCanvases();
                Changes = false;

                RectTransform t  = RootFolder.GetComponent<RectTransform>();
                float childWidth = t.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().bounds.max.x -
                                   t.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().bounds.min.x;
                float horizontalPadding =
                    t.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition.x; // TODO
                t.offsetMax = new Vector2((horizontalPadding * 2) + childWidth, t.offsetMax.y);

                float heightAccum = t.rect.height + Padding;
                for (int i = 0; i < RootFolder.Items.Count; i++) {
                    if (RootFolder.Items[i].item.Visible) {
                        float tabSize = RootFolder.Items[i].item.Depth * TabSize;
                        t             = RootFolder.Items[i].item.GetComponent<RectTransform>();

                        // t.offsetMin = new Vector2(tabSize, t.offsetMin.y);
                        // t.localPosition = new Vector3(t.localPosition.x, -heightAccum, t.localPosition.z);

                        // t.offsetMin = new Vector2(tabSize, t.offsetMin.y);

                        // childWidth = t.transform.GetChild(0).GetComponent<RectTransform>().offsetMax.x;
                        childWidth = t.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().bounds.max.x -
                                     t.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().bounds.min.x;
                        horizontalPadding =
                            t.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition.x; // TODO
                        t.offsetMax = new Vector2((horizontalPadding * 2) + childWidth + t.offsetMin.x, t.offsetMax.y);

                        t.localPosition = new Vector3(tabSize, -heightAccum, t.localPosition.z);
                        heightAccum += t.rect.height + Padding;
                    }
                }
                var contentRect       = contentContainer.GetComponent<RectTransform>();
                contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, heightAccum);
            }
        }
    }
}