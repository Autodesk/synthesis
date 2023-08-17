using System.Net.NetworkInformation;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Synthesis.Attributes;
using Synthesis.Util;
using TMPro;

#nullable enable

namespace Synthesis.UI.Hierarchy.HierarchyItems {
    public class HierarchyItem : InteractableObject {
        public delegate void OnClickEvent();
        public event OnClickEvent? OnItemClicked;

#region Properties

        public bool IsInherited { get => this.GetType() != typeof(HierarchyItem); }

        public int Index {
            get {
                if (Parent == null)
                    return 0;
                return Parent.GetIndex(this);
            }
        }
        public int LocalIndex {
            get {
                if (Parent == null)
                    return 0;
                return Parent.GetLocalIndex(this);
            }
        }
        public int GlobalIndex {
            get {
                if (Parent == null)
                    return 0;
                return Parent.GetGlobalIndex(this);
            }
        }
        public int Depth {
            get {
                if (Parent == null)
                    return 0;
                return Parent.Depth + 1;
            }
        }
        public TMP_Text TitleText = null!;
        public HierarchyFolderItem? Parent;
        private string title = String.Empty;
        public string Title {
            get => title;
            set {
                title               = value;
                TitleText.text      = title;
                gameObject.name     = title;
                base.ContextMenuUID = title;
            }
        }
        private bool visible = true;
        public bool Visible {
            get => visible;
            set {
                visible = value;
                SetVisible(visible);
            }
        }

        private Image? imageComponent           = null;
        private readonly Color NormalColor      = SynthesisUtil.ColorFromHex(0xf3f3f3ff);
        private readonly Color InteractionColor = SynthesisUtil.ColorFromHex(0xe8cc2aff);

#endregion

        // Use this for initialization
        protected void Awake() {
            var button = GetComponent<Button>();
            button.onClick.AddListener(() => { OnItemClicked?.Invoke(); });
        }

        protected override sealed void OnInteraction(bool isInteractedWith) {
            if (imageComponent == null)
                imageComponent = GetComponent<Image>();

            if (isInteractedWith)
                imageComponent.color = InteractionColor;
            else
                imageComponent.color = NormalColor;
        }

#region Hierarchy

        public virtual void Init(string title, HierarchyFolderItem? parent) {
            Title = title;
            if (parent != null) {
                parent.Add(this);
            }
            Visible = true;
        }

        protected virtual void SetVisible(bool visible) {
            gameObject.SetActive(visible);
        }

        public virtual void Remove() {
            HierarchyFolderItem parent = Parent!;
            while (parent != null) {
                int num = parent.Items.RemoveAll(x => x.item.gameObject == this.gameObject);
                parent  = parent.Parent!;
            }
            Parent            = null;
            Visible           = false;
            Hierarchy.Changes = true;
        }

#endregion

#region ContextMenu

        [ContextMenuOption("Remove", "CloseIcon")]
        public void RemoveContextMenu() {
            Remove();
        }

#endregion
    }
}