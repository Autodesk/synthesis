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

            try {
                ModalDynamic modal = (ModalDynamic)Activator.CreateInstance(typeof(T), args);
                modal.Init(unityObject);
                modal.Create();
                return false;
            } catch (Exception e) {
                Logger.Log($"Failed to create Modal of type '{typeof(T).FullName}'");
            }
            return true;
        }

        // public static GameObject GetActiveModalGameObject()
        //     => ActiveModalGameObject;
    }
}
