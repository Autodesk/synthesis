using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.Utilities;

using Logger = SynthesisAPI.Utilities.Logger;

namespace Synthesis.UI.Dynamic {
    public static class DynamicUIManager {

        public static ModalDynamic ActiveModal { get; private set; }
        public static PanelDynamic ActivePanel { get; private set; }
        // public static GameObject ActiveModalGameObject;

        public static bool CreateModal<T>(params object[] args) where T : ModalDynamic {

            var unityObject = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("dynamic-modal-base"), GameObject.Find("UI").transform.Find("ScreenSpace").Find("ModalContainer"));

            // var c = ColorManager.GetColor("SAMPLE");

            ModalDynamic modal = (ModalDynamic)Activator.CreateInstance(typeof(T), args);
            modal.Create_Internal(unityObject);
            modal.Create();

            if (ActiveModal != null)
                CloseActiveModal();
            
            ActiveModal = modal;

            SynthesisAssetCollection.BlurVolumeStatic.weight = 1f;
            return true;
        }

        // Currently only going to allow one active panel
        public static bool CreatePanel<T>(params object[] args) where T : PanelDynamic {
            var unityObject = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("dynamic-panel-base"), GameObject.Find("UI").transform.Find("ScreenSpace").Find("PanelContainer"));

            // var c = ColorManager.GetColor("SAMPLE");

            PanelDynamic panel = (PanelDynamic)Activator.CreateInstance(typeof(T), args);
            panel.Create_Internal(unityObject);
            panel.Create();
            
            if (ActivePanel != null)
                CloseActivePanel();

            ActivePanel = panel;

            return true;
        }
        
        public static bool CloseActiveModal() {
            if (ActiveModal == null) {
                return false;
            }

            ActiveModal.Delete();
            ActiveModal.Delete_Internal();

            ActiveModal = null;

            SynthesisAssetCollection.BlurVolumeStatic.weight = 0f;
            return true;
        }

        public static bool CloseActivePanel() {
            if (ActivePanel == null)
                return false;

            ActivePanel.Delete();
            ActivePanel.Delete_Internal();

            ActivePanel = null;
            return true;
        }

        public static void Update() {
            if (ActiveModal != null)
                ActiveModal.Update();
            if (ActivePanel != null)
                ActivePanel.Update();
        }

        public static T ApplyTemplate<T>(this T component, Func<T, T> template) where T : UIComponent
            => template(component);
        public static T ApplyTemplate<T>(this T component, Func<UIComponent, UIComponent> template) where T : UIComponent
            => template(component) as T;
        
        public static Rect GetOffsetRect(this RectTransform trans) {
            var min = new Vector2(trans.anchoredPosition.x + trans.rect.xMin, trans.anchoredPosition.y + trans.rect.yMin);
            var max = new Vector2(trans.anchoredPosition.x + trans.rect.xMax, trans.anchoredPosition.y + trans.rect.yMax);
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }
    }
}