using Synthesis.UI.Bars;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Synthesis.UI.Tabs {
    public abstract class Tab {
        public TabButton CreateButton(string name, Sprite sprite, UnityAction callback) {
            var obj = Object.Instantiate(
                NavigationBar.Instance.TabPanelButtonPrefab, NavigationBar.Instance.ModalTab.transform);
            var button                = obj.GetComponent<TabButton>();
            button.ButtonImage.sprite = sprite;
            button.Name               = name;
            button.SetCallback(callback);
            return button;
        }

        public void CreateDivider() {
            Object.Instantiate(NavigationBar.Instance.TabDividerPrefab, NavigationBar.Instance.ModalTab.transform);
        }

        public abstract void Create();
    }
}
