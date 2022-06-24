using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.Utilities;

using Logger = SynthesisAPI.Utilities.Logger;

namespace Synthesis.UI.Dynamic {
    public static class ModalManager {

        public static ModalDynamic ActiveModal { get; private set; }
        // public static GameObject ActiveModalGameObject;

        public static bool CreateModal<T>(params object[] args) where T : ModalDynamic {

            var unityObject = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("dynamic-modal-base"), GameObject.Find("UI").transform);

            // var c = ColorManager.GetColor("SAMPLE");

            ModalDynamic modal = (ModalDynamic)Activator.CreateInstance(typeof(T), args);
            modal.Create_Internal(unityObject);
            modal.Create();
            ActiveModal = modal;

            SynthesisAssetCollection.BlurVolumeStatic.weight = 1f;
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
