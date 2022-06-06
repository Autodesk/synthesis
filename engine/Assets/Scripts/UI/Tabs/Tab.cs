using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Synthesis.UI.Bars;
using UnityEngine.Events;

namespace Synthesis.UI.Tabs {
    public abstract class Tab {

        // public List<TabPanelButton> PanelButtons = new List<TabPanelButton>();

        public TabButton CreateButton(string name, Sprite sprite, UnityAction callback) {
            var obj = Object.Instantiate(NavigationBar.Instance.TabPanelButtonPrefab, NavigationBar.Instance.ModalTab.transform);
            var button = obj.GetComponent<TabButton>();
            button.ButtonImage.sprite = sprite;
            button.Name = name;
            button.SetCallback(callback);
            return button;
        }

        public void CreateDivider() {
            Object.Instantiate(NavigationBar.Instance.TabDividerPrefab, NavigationBar.Instance.ModalTab.transform);
        }

        public abstract void Create();
    }
}
