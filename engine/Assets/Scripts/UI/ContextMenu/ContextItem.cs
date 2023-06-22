using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.UI.ContextMenus {
    public class ContextItem : MonoBehaviour {
        public TMP_Text TextObj;
        public Image IconImage;

        public InteractableObject Creator;
        private Action<object> callback;
        public Action<object> Callback {
            get => callback;
            set => callback = value;
        }
        public Sprite Icon {
            get => IconImage.sprite;
            set => IconImage.sprite = value;
        }
        public string Text {
            get => TextObj.text;
            set {
                TextObj.text    = value;
                gameObject.name = value;
            }
        }

        public void OnClick() {
            ContextMenu.Hide();
            callback(Creator);
        }
    }
}
