using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.Utilities;

using Logger = SynthesisAPI.Utilities.Logger;

namespace Synthesis.UI.Dynamic {
    public static class ModalManager {

        public static ModalDynamic ActiveModal;
        // public static GameObject ActiveModalGameObject;

        public static bool CreateModal<T>(params object[] args) where T : ModalDynamic {

            var unityObject = GameObject.Instantiate(SynthesisAssetCollection.GetModalPrefab("dynamic-modal-base"), GameObject.Find("UI").transform);

            // var c = ColorManager.GetColor("SAMPLE");

            ModalDynamic modal = (ModalDynamic)Activator.CreateInstance(typeof(T), args);
            modal.Init(unityObject);
            modal.Create();
            ActiveModal = modal;
            return true;
        }

        public static void CloseModal()
        {
            // not sure if this is how this should be done
            ActiveModal.Delete();
            ActiveModal = null;
        }

        // public static GameObject GetActiveModalGameObject()
        //     => ActiveModalGameObject;

        public static T ApplyTemplate<T>(this T component, Func<T, T> template) where T : UIComponent
            => template(component);
        public static T ApplyTemplate<T>(this T component, Func<UIComponent, UIComponent> template) where T : UIComponent
            => template(component) as T;
    }
}
