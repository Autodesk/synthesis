using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Synthesis.Attributes;
using TMPro;

#nullable enable

namespace Synthesis.UI.Hierarchy.HierarchyItems {
    public class HierarchyFolderItem : HierarchyItem {
#region Properties

        private Image? image;
        private bool collapsed = false;
        public bool Collapsed {
            get => collapsed;
            set {
                collapsed = value;
                SetChildrenVisible(!collapsed);
                Hierarchy.Changes = true;

                // TODO: Replace with sprite indication
                // if (collapsed) {
                //     image.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                // } else {
                //     image.color = new Color(0.9529f, 0.9529f, 0.9529f, 1f);
                // }
            }
        }
        public int ChildrenCount                            => Items.Count;
        public List<(bool owned, HierarchyItem item)> Items  = new List<(bool, HierarchyItem)>();

#endregion

        public new void Awake() { base.Awake();
    }

    public void Start() {
        image = GetComponent<Image>();

        OnItemClicked += () => Collapsed = !Collapsed;
    }

#region Hierarchy

    public void Insert(int localIndex, HierarchyItem item, bool isOwned = true) {
        int parsedIndex = localIndex;
        if (isOwned) {
            item.Parent = this;
            for (int i = 0; i <= parsedIndex && i < Items.Count; i++) {
                if (!Items[i].owned) {
                    // i--;
                    parsedIndex++;
                }
            }
        }
        // Debug.Log($"[{Title}]Inserting at {parsedIndex}");
        Items.Insert(parsedIndex, (isOwned, item));
        if (Parent != null) {
            Parent.Insert(parsedIndex + LocalIndex + 1, item, false);
        } else {
            Hierarchy.Changes = true;
            Visible           = true;
        }
    }

    public void Add(HierarchyItem item) {
        item.Parent = this;
        Items.Add((true, item));
        if (Parent != null) {
            Parent.Insert(Items.Count + LocalIndex, item, false);
        } else {
            Hierarchy.Changes = true;
            Visible           = true;
        }
        if (item is HierarchyFolderItem) {
            HierarchyFolderItem folder = (HierarchyFolderItem)item;
            for (int i = 0; i < folder.Items.Count; i++) {
                Insert(i + folder.LocalIndex + 1, folder.Items[i].item, false);
                // Debug.Log("Inserting");
            }
        }
    }

    public override void Remove() {

        if (Parent == null) {
            Debug.Log("Can't remove root folder");
            return;
        }

        HierarchyFolderItem parent = Parent!;
        while (parent != null) {
            foreach (var item in Items) {
                parent.Items.RemoveAll(x => x.item.gameObject == item.item.gameObject);
            }
            parent = parent.Parent!;
        }
        base.Remove();
    }

    public HierarchyItem CreateItem(string title) {
        var item = Instantiate(Hierarchy.ItemPrefab, Hierarchy.ContentContainer.gameObject.transform)
                       .GetComponent<HierarchyItem>();
        item.Init(title, this);
        return item;
    }

    public HierarchyFolderItem CreateFolder(string title) {
        var folder = Instantiate(Hierarchy.FolderPrefab, Hierarchy.ContentContainer.gameObject.transform)
                         .GetComponent<HierarchyFolderItem>();
        folder.Init(title, this);
        return folder;
    }

    protected override void SetVisible(bool visible) {
        base.SetVisible(visible);
        if (!collapsed)
            SetChildrenVisible(visible);
    }

    public void SetChildrenVisible(bool visible) {
        foreach (var item in Items) {
            if (item.owned) {
                item.item.Visible = visible;
            }
        }
    }

    // I really hope these actually work
    public int GetIndex(HierarchyItem item) {
        int skipCount = 0;
        int index     = -1;
        for (int i = 0; i < Items.Count; i++) {
            if (Items[i].owned) {
                if (Items[i].item.gameObject == item.gameObject) {
                    index = i;
                    break;
                }
            } else {
                skipCount++;
            }
        }
        if (index == -1)
            return -1;

        return index - skipCount;
    }
    public int GetLocalIndex(HierarchyItem item)  => Items.FindIndex(x => x.item.gameObject == item.gameObject);
    public int GetGlobalIndex(HierarchyItem item) => Parent == null
                                                         ? GetLocalIndex(item) + 1
                                                         : Parent.GetGlobalIndex(this) + GetLocalIndex(item) + 1;

    public void DebugPrint() {
        print(Title);
        foreach (var i in Items) {
            string prefix = "";
            for (int x = 0; x < i.item.Depth; x++) {
                prefix += "-";
            }
            print($"[{i.item.Index}][{i.item.LocalIndex}]{prefix}{i.item.Title}");
        }
    }

#endregion

#region ContextMenu

    [ContextMenuOption("Remove", "CloseIcon")]
    public void RemoveContextMenu2() {
        Remove();
    }

    [ContextMenuOption("Toggle Collapse", "CollapseIcon")]
    public void ToggleCollapseContextMenu() {
        Collapsed = !Collapsed;
    }

#endregion

}
}
